using Auth.Application.Abstractions.Messaging.Application.Abstractions.Messaging;

namespace Auth.Application.UseCases.Auth.Commands.RefreshToken
{
    public sealed record RefreshTokenCommand(
        string UserId,
        string RefreshToken
    ) : ICommand<RefreshTokenResponse>;
}
