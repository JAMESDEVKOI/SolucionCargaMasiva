using Auth.Application.UseCases.Auth.Commands.Login;
using Auth.Application.UseCases.Auth.Commands.Logout;
using Auth.Application.UseCases.Auth.Commands.RefreshToken;
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
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionId = User.FindFirstValue("SessionId");
            var accessToken = HttpContext.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(sessionId))
            {
                return BadRequest(new { error = "Auth.InvalidToken", message = "Token inválido" });
            }

            var command = new LogoutCommand(userId, sessionId, accessToken);
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error.Code, message = result.Error.Message });
            }

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
    }
}
