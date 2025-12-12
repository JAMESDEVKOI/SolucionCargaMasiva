using Auth.Application.Abstractions.Messaging.Application.Abstractions.Messaging;

namespace Auth.Application.UseCases.Auth.Commands.Login
{
    public sealed record LoginCommand(
       string Email,
       string Password,
       string? IpAddress = null,
       string? UserAgent = null
   ) : ICommand<LoginResponse>;
}
