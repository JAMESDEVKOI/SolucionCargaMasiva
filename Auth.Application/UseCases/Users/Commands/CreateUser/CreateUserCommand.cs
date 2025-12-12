using Auth.Application.Abstractions.Messaging.Application.Abstractions.Messaging;

namespace Auth.Application.UseCases.Users.Commands.CreateUser
{
    public sealed record CreateUserCommand(
      string Name,
      string LastName,
      string Email,
      string Password,
      string? Phone = null
     ) : ICommand<Guid>;
}
