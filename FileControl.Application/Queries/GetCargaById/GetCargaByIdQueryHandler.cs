using FileControl.Application.DTOs;
using FileControl.Application.Interfaces;
using FileControl.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FileControl.Application.Queries.GetCargaById
{
    public class GetCargaByIdQueryHandler : IRequestHandler<GetCargaByIdQuery, CargaArchivoListDto?>
    {
        private readonly ICargaArchivoRepository _repository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCargaByIdQueryHandler> _logger;

        public GetCargaByIdQueryHandler(
            ICargaArchivoRepository repository,
            ICurrentUserService currentUserService,
            ILogger<GetCargaByIdQueryHandler> logger)
        {
            _repository = repository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<CargaArchivoListDto?> Handle(GetCargaByIdQuery request, CancellationToken cancellationToken)
        {
            var usuario = _currentUserService.Email;

            if (string.IsNullOrEmpty(usuario))
            {
                throw new UnauthorizedAccessException("No se pudo obtener el usuario");
            }

            _logger.LogInformation("Obteniendo carga {IdCarga} para usuario: {Usuario}", request.Id, usuario);

            // Obtener la carga
            var carga = await _repository.GetByIdAsync(request.Id, cancellationToken);

            // Verificar que existe y pertenece al usuario actual
            if (carga == null)
            {
                _logger.LogWarning("Carga {IdCarga} no encontrada", request.Id);
                return null;
            }

            // Verificar que la carga pertenece al usuario actual
            if (carga.Usuario != usuario)
            {
                _logger.LogWarning("Usuario {Usuario} intentó acceder a carga {IdCarga} de {UsuarioCarga}",
                    usuario, request.Id, carga.Usuario);
                throw new UnauthorizedAccessException("No tiene permisos para ver esta carga");
            }

            // Mapear a DTO
            var result = new CargaArchivoListDto(
                Id: carga.Id,
                NombreArchivo: carga.NombreArchivo,
                Usuario: carga.Usuario,
                Periodo: carga.Periodo,
                Estado: carga.Estado.ToString(),
                FechaRegistro: carga.FechaRegistro,
                FechaInicioProceso: carga.FechaInicioProceso,
                FechaFinProceso: carga.FechaFinProceso,
                TotalRegistros: carga.TotalRegistros,
                RegistrosProcesados: carga.RegistrosProcesados,
                RegistrosExitosos: carga.RegistrosExitosos,
                RegistrosFallidos: carga.RegistrosFallidos,
                MensajeError: carga.MensajeError
            );

            _logger.LogInformation("Carga {IdCarga} encontrada. Estado: {Estado}", request.Id, carga.Estado);

            return result;
        }
    }
}
