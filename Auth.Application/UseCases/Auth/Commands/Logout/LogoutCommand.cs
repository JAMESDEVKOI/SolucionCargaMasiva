using Auth.Application.Abstractions.Messaging.Application.Abstractions.Messaging;

namespace Auth.Application.UseCases.Auth.Commands.Logout
{
    public sealed record LogoutCommand(
        string UserId,
        string SessionId,
        string? AccessToken = null
    ) : ICommand;
}
