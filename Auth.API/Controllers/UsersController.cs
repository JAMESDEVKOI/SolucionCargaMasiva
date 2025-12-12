using Auth.Application.UseCases.Users.Commands.CreateUser;
using Auth.Application.UseCases.Users.Queries.GetAllUsers;
using Auth.Application.UseCases.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ISender _sender;

        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error.Code, message = result.Error.Message });
            }

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = result.Value },
                result.Value);
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAllUsersQuery(page, pageSize);
            var result = await _sender.Send(query, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error.Code, message = result.Error.Message });
            }

            return Ok(result.Value);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery(id);
            var result = await _sender.Send(query, cancellationToken);

            if (result.IsFailure)
            {
                return NotFound(new { error = result.Error.Code, message = result.Error.Message });
            }

            return Ok(result.Value);
        }
    }
}
