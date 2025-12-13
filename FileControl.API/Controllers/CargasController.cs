using FileControl.Application.Commands.CreateCargaArchivo;
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
