using FileControl.Application.DTOs;
using FileControl.Application.Interfaces;
using FileControl.Domain.Entities;
using FileControl.Domain.Enums;
using FileControl.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FileControl.Application.Commands.CreateCargaArchivo
{
    public class CreateCargaArchivoCommandHandler : IRequestHandler<CreateCargaArchivoCommand, CargaArchivoResponseDto>
    {
        private readonly ICargaArchivoRepository _repository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMessageBusService _messageBusService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateCargaArchivoCommandHandler> _logger;

        public CreateCargaArchivoCommandHandler(
            ICargaArchivoRepository repository,
            IFileStorageService fileStorageService,
            IMessageBusService messageBusService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ILogger<CreateCargaArchivoCommandHandler> logger)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _messageBusService = messageBusService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CargaArchivoResponseDto> Handle(
            CreateCargaArchivoCommand request,
            CancellationToken cancellationToken)
        {
            // Validar permisos
            if (!_currentUserService.HasPermission("BulkUpload"))
            {
                throw new UnauthorizedAccessException("No tiene permisos para realizar cargas masivas");
            }

            var userEmail = _currentUserService.Email
                ?? throw new InvalidOperationException("No se pudo obtener el email del usuario");

            _logger.LogInformation(
                "Iniciando proceso de carga masiva. Usuario: {Usuario}, Archivo: {Archivo}, Periodo: {Periodo}",
                userEmail, request.File.FileName, request.Periodo);

            // Crear registro inicial
            var cargaArchivo = new CargaArchivo
            {
                NombreArchivo = request.File.FileName,
                Usuario = userEmail,
                Periodo = request.Periodo,
                Estado = CargaEstado.Pendiente,
                FechaRegistro = DateTime.UtcNow,
                TotalRegistros = 0,
                RegistrosProcesados = 0,
                RegistrosExitosos = 0,
                RegistrosFallidos = 0
            };

            // Guardar en base de datos
            var cargaCreada = await _repository.CreateAsync(cargaArchivo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Registro de carga creado con ID: {IdCarga}", cargaCreada.Id);

            try
            {
                // Subir archivo a SeaweedFS
                using var stream = request.File.OpenReadStream();
                var fileId = await _fileStorageService.UploadFileAsync(
                    stream,
                    request.File.FileName,
                    cancellationToken);

                _logger.LogInformation(
                    "Archivo subido a SeaweedFS con FileId: {FileId}",
                    fileId);

                // Actualizar registro con fileId
                cargaCreada.FileId = fileId;
                await _repository.UpdateAsync(cargaCreada, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publicar mensaje en RabbitMQ
                var message = new CargaMasivaMessageDto(
                    IdCarga: cargaCreada.Id,
                    FileId: fileId,
                    NombreArchivo: request.File.FileName,
                    Usuario: userEmail,
                    Periodo: request.Periodo
                );

                await _messageBusService.PublishCargaMasivaAsync(message, cancellationToken);

                _logger.LogInformation(
                    "Mensaje publicado en RabbitMQ para IdCarga: {IdCarga}",
                    cargaCreada.Id);

                return new CargaArchivoResponseDto(
                    IdCarga: cargaCreada.Id,
                    Estado: cargaCreada.Estado.ToString()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al procesar la carga. IdCarga: {IdCarga}",
                    cargaCreada.Id);

                // Actualizar estado a Fallida
                cargaCreada.Estado = CargaEstado.Fallida;
                cargaCreada.MensajeError = ex.Message;
                await _repository.UpdateAsync(cargaCreada, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                throw;
            }
        }
    }
}
