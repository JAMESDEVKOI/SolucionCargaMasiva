namespace Auth.Application.UseCases.Auth.Commands.RefreshToken
{
    public sealed record RefreshTokenResponse(
        string AccessToken,
        string RefreshToken
    );
}
