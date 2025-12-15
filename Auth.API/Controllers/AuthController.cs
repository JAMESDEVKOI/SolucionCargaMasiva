using Auth.Application.UseCases.Auth.Commands.Login;
using Auth.Application.UseCases.Auth.Commands.Logout;
using Auth.Application.UseCases.Auth.Commands.RefreshToken;
using Auth.Application.UseCases.Auth.Queries.GetCurrentUser;
using Auth.Application.UseCases.Users.Commands.CreateUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(
            [FromBody] CreateUserCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error.Code, message = result.Error.Message });
            }

            return CreatedAtAction(
                nameof(Register),
                new { id = result.Value },
                result.Value);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command,
            CancellationToken cancellationToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var loginCommand = command with
            {
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            var result = await _sender.Send(loginCommand, cancellationToken);

            if (result.IsFailure)
            {
                return Unauthorized(new { error = result.Error.Code, message = result.Error.Message });
            }

            return Ok(result.Value);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            // Intentar obtener información del usuario desde los claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionId = User.FindFirstValue("SessionId");

            // Obtener el token de la cookie o del header Authorization
            var accessToken = HttpContext.Request.Cookies["access_token"];
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = HttpContext.Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");
            }

            // Si tenemos userId y sessionId, intentar invalidar la sesión en el backend
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var command = new LogoutCommand(userId, sessionId, accessToken);
                    await _sender.Send(command, cancellationToken);
                    // Ignoramos el resultado - si falla, igual continuamos
                }
                catch
                {
                    // Ignoramos errores - el usuario puede hacer logout incluso si falla la invalidación
                }
            }

            // SIEMPRE limpiar las cookies, sin importar si el token era válido o no
            HttpContext.Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            HttpContext.Response.Cookies.Delete("refresh_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            // SIEMPRE retornar éxito
            return NoContent();
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return Unauthorized(new { error = result.Error.Code, message = result.Error.Message });
            }

            return Ok(result.Value);
        }

        [HttpGet("me")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var query = new GetCurrentUserQuery();
            var result = await _sender.Send(query, cancellationToken);

            if (result.IsFailure)
            {
                return Unauthorized(new { error = result.Error.Code, message = result.Error.Message });
            }

            return Ok(result.Value);
        }
    }
}
