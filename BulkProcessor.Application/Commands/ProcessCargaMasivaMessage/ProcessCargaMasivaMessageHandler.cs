using BulkProcessor.Application.DTOs;
using BulkProcessor.Application.Interfaces;
using BulkProcessor.Domain.Entities;
using BulkProcessor.Domain.Enums;
using BulkProcessor.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BulkProcessor.Application.Commands.ProcessCargaMasivaMessage
{
    public class ProcessCargaMasivaMessageHandler : IRequestHandler<ProcessCargaMasivaMessageCommand, bool>
    {
        private readonly ICargaArchivoRepository _cargaRepository;
        private readonly IDataProcesadaRepository _dataRepository;
        private readonly ICargaFalloRepository _falloRepository;
        private readonly IFileStorageService _fileStorage;
        private readonly IExcelParserService _excelParser;
        private readonly IMessageBusService _messageBus;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessCargaMasivaMessageHandler> _logger;

        public ProcessCargaMasivaMessageHandler(
            ICargaArchivoRepository cargaRepository,
            IDataProcesadaRepository dataRepository,
            ICargaFalloRepository falloRepository,
            IFileStorageService fileStorage,
            IExcelParserService excelParser,
            IMessageBusService messageBus,
            IUnitOfWork unitOfWork,
            ILogger<ProcessCargaMasivaMessageHandler> logger)
        {
            _cargaRepository = cargaRepository;
            _dataRepository = dataRepository;
            _falloRepository = falloRepository;
            _fileStorage = fileStorage;
            _excelParser = excelParser;
            _messageBus = messageBus;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> Handle(
            ProcessCargaMasivaMessageCommand request,
            CancellationToken cancellationToken)
        {
            var message = request.Message;

            _logger.LogInformation(
                "Iniciando procesamiento de carga. IdCarga: {IdCarga}, Periodo: {Periodo}, Usuario: {Usuario}",
                message.IdCarga, message.Periodo, message.Usuario);

            try
            {
                // 1. Obtener carga actual
                var carga = await _cargaRepository.GetByIdAsync(message.IdCarga, cancellationToken);
                if (carga == null)
                {
                    _logger.LogWarning("Carga {IdCarga} no encontrada", message.IdCarga);
                    return false;
                }

                // 2. VALIDAR IDEMPOTENCIA - No reprocesar si ya está finalizado/rechazado/notificado
                if (carga.Estado == CargaEstado.Finalizado ||
                    carga.Estado == CargaEstado.Rechazado ||
                    carga.Estado == CargaEstado.Notificado)
                {
                    _logger.LogInformation(
                        "Carga {IdCarga} ya está en estado {Estado}. Ignorando mensaje (idempotencia).",
                        message.IdCarga, carga.Estado);
                    return true; // ACK el mensaje sin reprocesar
                }

                // 3. VALIDACIÓN CRÍTICA: Duplicidad por PERIODO (con transacción)
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    var estadosActivos = new[] {
                        CargaEstado.Cargado,
                        CargaEstado.Finalizado,
                        CargaEstado.Notificado
                    };

                    var existeActivaFinalizada = await _cargaRepository.ExistsByPeriodoAndEstadoAsync(
                        message.Periodo,
                        estadosActivos,
                        message.IdCarga,
                        cancellationToken);

                    if (existeActivaFinalizada)
                    {
                        _logger.LogWarning(
                            "Rechazando carga {IdCarga}. Periodo {Periodo} ya procesado previamente",
                            message.IdCarga, message.Periodo);

                        await RechazarCargaAsync(
                            carga,
                            "Periodo ya procesado",
                            message.Usuario,
                            cancellationToken);

                        await _unitOfWork.CommitTransactionAsync(cancellationToken);
                        return true;
                    }

                    var estadosPendientes = new[] {
                        CargaEstado.Pendiente,
                        CargaEstado.EnProceso
                    };

                    var existePendiente = await _cargaRepository.ExistsByPeriodoAndEstadoAsync(
                        message.Periodo,
                        estadosPendientes,
                        message.IdCarga,
                        cancellationToken);

                    if (existePendiente)
                    {
                        _logger.LogWarning(
                            "Rechazando carga {IdCarga}. Periodo {Periodo} está siendo procesado simultáneamente",
                            message.IdCarga, message.Periodo);

                        await RechazarCargaAsync(
                            carga,
                            "Periodo en proceso o pendiente",
                            message.Usuario,
                            cancellationToken);

                        await _unitOfWork.CommitTransactionAsync(cancellationToken);
                        return true;
                    }

                    // 4. Actualizar estado a EnProceso
                    carga.Estado = CargaEstado.EnProceso;
                    carga.FechaInicioProceso = DateTime.UtcNow;
                    await _cargaRepository.UpdateAsync(carga, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    _logger.LogInformation(
                        "Carga {IdCarga} actualizada a estado EnProceso",
                        message.IdCarga);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                // 5. Descargar archivo desde SeaweedFS
                _logger.LogInformation(
                    "Descargando archivo desde SeaweedFS. FileId: {FileId}",
                    message.FileId);

                Stream fileStream;
                try
                {
                    fileStream = await _fileStorage.DownloadFileAsync(message.FileId, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error al descargar archivo desde SeaweedFS. FileId: {FileId}",
                        message.FileId);

                    await RechazarCargaAsync(
                        carga,
                        $"Error al descargar archivo: {ex.Message}",
                        message.Usuario,
                        cancellationToken);

                    return true;
                }

                // 6. Procesar Excel
                _logger.LogInformation("Parseando archivo Excel");

                IEnumerable<ExcelRowDto> rows;
                try
                {
                    rows = await _excelParser.ParseExcelAsync(fileStream, cancellationToken);
                    await fileStream.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al parsear archivo Excel");

                    await RechazarCargaAsync(
                        carga,
                        $"Error al parsear Excel: {ex.Message}",
                        message.Usuario,
                        cancellationToken);

                    return true;
                }

                var rowsList = rows.ToList();
                carga.TotalRegistros = rowsList.Count;

                _logger.LogInformation(
                    "Excel parseado. Total de filas: {TotalFilas}",
                    rowsList.Count);

                // 7. Procesar filas: validar duplicados e insertar
                var dataToInsert = new List<DataProcesada>();
                var fallos = new List<CargaFallo>();
                int procesados = 0;

                foreach (var row in rowsList)
                {
                    procesados++;

                    // Validar CodigoProducto requerido
                    if (string.IsNullOrWhiteSpace(row.CodigoProducto))
                    {
                        fallos.Add(new CargaFallo
                        {
                            IdCarga = message.IdCarga,
                            RowNumber = row.RowNumber,
                            CodigoProducto = null,
                            Motivo = "Campo requerido: CodigoProducto vacío",
                            RawData = JsonSerializer.Serialize(row.RawData),
                            CreatedAt = DateTime.UtcNow
                        });
                        continue;
                    }

                    // Validar duplicidad por CodigoProducto
                    var existe = await _dataRepository.ExistsByCodigoProductoAsync(
                        row.CodigoProducto,
                        cancellationToken);

                    if (existe)
                    {
                        fallos.Add(new CargaFallo
                        {
                            IdCarga = message.IdCarga,
                            RowNumber = row.RowNumber,
                            CodigoProducto = row.CodigoProducto,
                            Motivo = "Existente",
                            RawData = JsonSerializer.Serialize(row.RawData),
                            CreatedAt = DateTime.UtcNow
                        });
                        continue;
                    }

                    // Agregar a lista de inserción
                    dataToInsert.Add(new DataProcesada
                    {
                        Periodo = message.Periodo,
                        CodigoProducto = row.CodigoProducto,
                        NombreProducto = row.NombreProducto,
                        Categoria = row.Categoria,
                        Precio = row.Precio,
                        Stock = row.Stock,
                        Proveedor = row.Proveedor,
                        Descripcion = row.Descripcion,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                carga.RegistrosProcesados = procesados;
                carga.RegistrosFallidos = fallos.Count;

                // 8. Insertar datos válidos (bulk)
                int insertados = 0;
                if (dataToInsert.Any())
                {
                    _logger.LogInformation(
                        "Insertando {Count} registros válidos en DataProcesada",
                        dataToInsert.Count);

                    insertados = await _dataRepository.BulkInsertAsync(dataToInsert, cancellationToken);
                    carga.RegistrosExitosos = insertados;

                    _logger.LogInformation(
                        "{Insertados} registros insertados exitosamente",
                        insertados);
                }

                // 9. Insertar fallos (bulk)
                if (fallos.Any())
                {
                    _logger.LogInformation(
                        "Registrando {Count} fallos en CargaFallo",
                        fallos.Count);

                    await _falloRepository.BulkInsertAsync(fallos, cancellationToken);
                }

                // 10. Actualizar estado a Cargado, luego Finalizado
                carga.Estado = CargaEstado.Cargado;
                await _cargaRepository.UpdateAsync(carga, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                carga.Estado = CargaEstado.Finalizado;
                carga.FechaFinProceso = DateTime.UtcNow;
                await _cargaRepository.UpdateAsync(carga, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Carga {IdCarga} finalizada. Procesados: {Procesados}, Exitosos: {Exitosos}, Fallidos: {Fallidos}",
                    message.IdCarga, procesados, insertados, fallos.Count);

                // 11. Publicar notificación de éxito
                var notification = new CargaFinalizadaNotificationDto(
                    IdCarga: message.IdCarga,
                    Usuario: message.Usuario,
                    FechaFin: DateTime.UtcNow,
                    Estado: "Finalizado",
                    Procesados: procesados,
                    Insertados: insertados,
                    Fallidos: fallos.Count
                );

                await _messageBus.PublishNotificationAsync(notification, cancellationToken);

                _logger.LogInformation(
                    "Notificación de éxito publicada para carga {IdCarga}",
                    message.IdCarga);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error inesperado al procesar carga {IdCarga}",
                    message.IdCarga);

                try
                {
                    var carga = await _cargaRepository.GetByIdAsync(message.IdCarga, cancellationToken);
                    if (carga != null)
                    {
                        await RechazarCargaAsync(
                            carga,
                            $"Error inesperado: {ex.Message}",
                            message.Usuario,
                            cancellationToken);
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx,
                        "Error al intentar rechazar carga {IdCarga} después de fallo",
                        message.IdCarga);
                }

                throw; // Re-lanzar para NACK
            }
        }

        private async Task RechazarCargaAsync(
            CargaArchivo carga,
            string motivo,
            string usuario,
            CancellationToken cancellationToken)
        {
            carga.Estado = CargaEstado.Rechazado;
            carga.FechaFinProceso = DateTime.UtcNow;
            carga.MensajeError = motivo;

            await _cargaRepository.UpdateAsync(carga, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var notification = new CargaFinalizadaNotificationDto(
                IdCarga: carga.Id,
                Usuario: usuario,
                FechaFin: DateTime.UtcNow,
                Estado: "Rechazado",
                Motivo: motivo
            );

            await _messageBus.PublishNotificationAsync(notification, cancellationToken);

            _logger.LogInformation(
                "Carga {IdCarga} rechazada. Motivo: {Motivo}",
                carga.Id, motivo);
        }
    }
}
