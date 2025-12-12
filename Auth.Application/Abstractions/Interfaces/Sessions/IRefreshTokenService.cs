namespace Auth.Application.Abstractions.Interfaces.Sessions
{
    public interface IRefreshTokenService
    {
        Task<string> GenerateRefreshTokenAsync(Guid userId, string ipAddress);

        Task<bool> ValidateRefreshTokenAsync(string token);

        Task<Guid?> GetUserIdFromRefreshTokenAsync(string token);

        Task RevokeRefreshTokenAsync(string token, string? ipAddress = null);

        Task RevokeAllUserRefreshTokensAsync(Guid userId, string? ipAddress = null);

        Task DeleteExpiredTokensAsync();

        Task<RefreshTokenInfo?> GetRefreshTokenInfoAsync(string token);
    }
}
