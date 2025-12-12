using Auth.Application.Abstractions.Interfaces.Caching;
using Auth.Application.Abstractions.Interfaces.Sessions;
using System.Security.Cryptography;

namespace Auth.Infrastructure.Sessions
{
    internal sealed class RefreshTokenService : IRefreshTokenService
    {
        private readonly ICacheService _cacheService;
        private const string RefreshTokenKeyPrefix = "refresh-token:";
        private const string UserTokensKeyPrefix = "user-tokens:";
        private static readonly TimeSpan RefreshTokenExpiry = TimeSpan.FromDays(7);

        public RefreshTokenService(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<string> GenerateRefreshTokenAsync(Guid userId, string ipAddress)
        {
            var token = GenerateSecureToken();
            var tokenInfo = new RefreshTokenInfo
            {
                UserId = userId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(RefreshTokenExpiry),
                CreatedByIp = ipAddress,
                IsRevoked = false
            };

            var tokenKey = GetTokenKey(token);
            await _cacheService.SetAsync(tokenKey, tokenInfo, RefreshTokenExpiry);

            var userTokensKey = GetUserTokensKey(userId);
            var userTokens = await _cacheService.GetAsync<HashSet<string>>(userTokensKey) ?? new HashSet<string>();
            userTokens.Add(token);
            await _cacheService.SetAsync(userTokensKey, userTokens, RefreshTokenExpiry);

            return token;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token)
        {
            var tokenKey = GetTokenKey(token);
            var tokenInfo = await _cacheService.GetAsync<RefreshTokenInfo>(tokenKey);

            return tokenInfo is not null && tokenInfo.IsActive;
        }

        public async Task<Guid?> GetUserIdFromRefreshTokenAsync(string token)
        {
            var tokenKey = GetTokenKey(token);
            var tokenInfo = await _cacheService.GetAsync<RefreshTokenInfo>(tokenKey);

            return tokenInfo?.IsActive == true ? tokenInfo.UserId : null;
        }

        public async Task RevokeRefreshTokenAsync(string token, string? ipAddress = null)
        {
            var tokenKey = GetTokenKey(token);
            var tokenInfo = await _cacheService.GetAsync<RefreshTokenInfo>(tokenKey);

            if (tokenInfo is not null)
            {
                tokenInfo.IsRevoked = true;
                tokenInfo.RevokedAt = DateTime.UtcNow;
                tokenInfo.RevokedByIp = ipAddress;
                await _cacheService.SetAsync(tokenKey, tokenInfo, TimeSpan.FromHours(1));

                var userTokensKey = GetUserTokensKey(tokenInfo.UserId);
                var userTokens = await _cacheService.GetAsync<HashSet<string>>(userTokensKey);
                if (userTokens is not null)
                {
                    userTokens.Remove(token);
                    await _cacheService.SetAsync(userTokensKey, userTokens, RefreshTokenExpiry);
                }
            }
        }

        public async Task RevokeAllUserRefreshTokensAsync(Guid userId, string? ipAddress = null)
        {
            var userTokensKey = GetUserTokensKey(userId);
            var userTokens = await _cacheService.GetAsync<HashSet<string>>(userTokensKey);

            if (userTokens is not null)
            {
                foreach (var token in userTokens)
                {
                    await RevokeRefreshTokenAsync(token, ipAddress);
                }
            }

            await _cacheService.RemoveAsync(userTokensKey);
        }

        public async Task DeleteExpiredTokensAsync()
        {
            await Task.CompletedTask;
        }

        public async Task<RefreshTokenInfo?> GetRefreshTokenInfoAsync(string token)
        {
            var tokenKey = GetTokenKey(token);
            return await _cacheService.GetAsync<RefreshTokenInfo>(tokenKey);
        }

        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private static string GetTokenKey(string token) => $"{RefreshTokenKeyPrefix}{token}";
        private static string GetUserTokensKey(Guid userId) => $"{UserTokensKeyPrefix}{userId}";
    }
}
