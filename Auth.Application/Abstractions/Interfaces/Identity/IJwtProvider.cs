using Auth.Domain.User;

namespace Auth.Application.Abstractions.Interfaces.Identity
{
    public interface IJwtProvider
    {
        Task<string> GenerateAccessTokenAsync(
            User user,
            CancellationToken cancellationToken = default);

        string GenerateRefreshToken();

        Task<bool> StoreRefreshTokenAsync(
            string userId,
            string refreshToken,
            CancellationToken cancellationToken = default);

        Task<bool> ValidateRefreshTokenAsync(
            string userId,
            string refreshToken,
            CancellationToken cancellationToken = default);

        Task<bool> RevokeTokenAsync(
            string token,
            CancellationToken cancellationToken = default);
    }
}
