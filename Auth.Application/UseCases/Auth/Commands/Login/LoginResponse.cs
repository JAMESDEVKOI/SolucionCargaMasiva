namespace Auth.Application.UseCases.Auth.Commands.Login
{
    public sealed record LoginResponse(
      string AccessToken,
      string RefreshToken,
      string SessionId,
      Guid UserId,
      string Email,
      string Name
  );
}
