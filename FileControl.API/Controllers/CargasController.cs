using FileControl.Application.Commands.CreateCargaArchivo;
using FileControl.Application.Queries.GetCargas;
using FileControl.Application.Queries.GetCargaById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileControl.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CargasController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly ILogger<CargasController> _logger;

        public CargasController(ISender sender, ILogger<CargasController> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        /// <summary>
        /// Obtener el historial de cargas masivas del usuario actual
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Lista de cargas del usuario</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCargas(CancellationToken cancellationToken)
        {
            var query = new GetCargasQuery();
            var result = await _sender.Send(query, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Obtener el detalle de una carga específica
        /// </summary>
        /// <param name="id">ID de la carga</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Detalle de la carga</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCargaById(int id, CancellationToken cancellationToken)
        {
            var query = new GetCargaByIdQuery(id);
            var result = await _sender.Send(query, cancellationToken);

            if (result == null)
            {
                return NotFound(new { mensaje = $"No se encontró la carga con ID {id}" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Crear una nueva carga masiva
        /// </summary>
        /// <param name="request">Datos de la carga (archivo y periodo)</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Información de la carga creada</returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCarga(
            [FromForm] FileControl.API.Models.CreateCargaRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateCargaArchivoCommand(request.File, request.Periodo);
            var result = await _sender.Send(command, cancellationToken);

            return Ok(result);
        }
    }
}
