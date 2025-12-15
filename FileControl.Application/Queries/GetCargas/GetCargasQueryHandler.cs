using FileControl.Application.DTOs;
using FileControl.Application.Interfaces;
using FileControl.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FileControl.Application.Queries.GetCargas
{
    public class GetCargasQueryHandler : IRequestHandler<GetCargasQuery, List<CargaArchivoListDto>>
    {
        private readonly ICargaArchivoRepository _repository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCargasQueryHandler> _logger;

        public GetCargasQueryHandler(
            ICargaArchivoRepository repository,
            ICurrentUserService currentUserService,
            ILogger<GetCargasQueryHandler> logger)
        {
            _repository = repository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<CargaArchivoListDto>> Handle(GetCargasQuery request, CancellationToken cancellationToken)
        {
            // Si no se especifica usuario, obtener del contexto actual
            var usuario = request.Usuario ?? _currentUserService.Email;

            if (string.IsNullOrEmpty(usuario))
            {
                throw new UnauthorizedAccessException("No se pudo obtener el usuario");
            }

            _logger.LogInformation("Obteniendo cargas para usuario: {Usuario}", usuario);

            // Obtener todas las cargas del usuario
            var cargas = await _repository.GetByUsuarioAsync(usuario, cancellationToken);

            // Mapear a DTO
            var result = cargas.Select(c => new CargaArchivoListDto(
                Id: c.Id,
                NombreArchivo: c.NombreArchivo,
                Usuario: c.Usuario,
                Periodo: c.Periodo,
                Estado: c.Estado.ToString(),
                FechaRegistro: c.FechaRegistro,
                FechaInicioProceso: c.FechaInicioProceso,
                FechaFinProceso: c.FechaFinProceso,
                TotalRegistros: c.TotalRegistros,
                RegistrosProcesados: c.RegistrosProcesados,
                RegistrosExitosos: c.RegistrosExitosos,
                RegistrosFallidos: c.RegistrosFallidos,
                MensajeError: c.MensajeError
            ))
            .OrderByDescending(c => c.FechaRegistro)
            .ToList();

            _logger.LogInformation("Se encontraron {Count} cargas para usuario: {Usuario}", result.Count, usuario);

            return result;
        }
    }
}
