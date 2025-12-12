using Auth.Application.Abstractions.Interfaces.Caching;
using Auth.Application.Abstractions.Interfaces.Data;
using Auth.Application.Abstractions.Interfaces.Identity;
using Auth.Domain.User;
using Auth.Infrastructure.Identity.Claims;
using Dapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Infrastructure.Identity.Authentication.Jwt
{
    internal sealed class JwtTokenProvider : IJwtProvider
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ICacheService _cacheService;

        public JwtTokenProvider(
            IOptions<JwtSettings> settings,
            ISqlConnectionFactory sqlConnectionFactory,
            ICacheService cacheService)
        {
            _jwtSettings = settings.Value;
            _sqlConnectionFactory = sqlConnectionFactory;
            _cacheService = cacheService;
        }

        public async Task<string> GenerateAccessTokenAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            if (user?.Id is null)
                throw new ArgumentNullException(nameof(user), "User or User.Id cannot be null");

            var permissions = await GetUserPermissionsAsync(
                user.Id.Value,
                cancellationToken);

            var claims = await CreateClaimsAsync(user, permissions, cancellationToken);

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        public async Task<bool> RevokeTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            var jti = GetTokenJti(token);
            if (string.IsNullOrEmpty(jti))
                return false;

            var key = $"blacklist:{jti}";
            var expiry = TimeSpan.FromMinutes(_jwtSettings.AccessTokenExpiryMinutes);

            await _cacheService.SetAsync(key, true, expiry, cancellationToken);
            return true;
        }

        public async Task<bool> StoreRefreshTokenAsync(
            string userId,
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            var key = $"refresh_token:{userId}";
            var expiry = TimeSpan.FromDays(_jwtSettings.RefreshTokenExpiryDays);

            await _cacheService.SetAsync(key, refreshToken, expiry, cancellationToken);
            return true;
        }

        public async Task<bool> ValidateRefreshTokenAsync(
            string userId,
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            var key = $"refresh_token:{userId}";
            var storedToken = await _cacheService.GetAsync<string>(key, cancellationToken);

            return storedToken == refreshToken;
        }

        private async Task<HashSet<string>> GetUserPermissionsAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"permissions:{userId}";

            var cachedPermissions = await _cacheService.GetAsync<HashSet<string>>(
                cacheKey,
                cancellationToken);

            if (cachedPermissions != null)
                return cachedPermissions;

            const string sql = """
                SELECT DISTINCT
                    p.name
                FROM "UserRoles" ur
                INNER JOIN roles r ON r.id = ur."RoleId"
                INNER JOIN roles_permissions rp ON rp.role_id = r.id
                INNER JOIN permissions p ON p.id = rp.permission_id
                WHERE ur."UserId" = @UserId
            """;

            using var connection = _sqlConnectionFactory.CreateConnection();
            var permissions = await connection.QueryAsync<string>(
                sql,
                new { UserId = userId });

            var permissionSet = permissions.ToHashSet();

            await _cacheService.SetAsync(
                cacheKey,
                permissionSet,
                TimeSpan.FromHours(1),
                cancellationToken);

            return permissionSet;
        }

        private async Task<HashSet<string>> GetUserRolesAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"roles:{userId}";

            var cachedRoles = await _cacheService.GetAsync<HashSet<string>>(
                cacheKey,
                cancellationToken);

            if (cachedRoles != null)
                return cachedRoles;

            const string sql = """
               SELECT DISTINCT
                    r.name
               FROM "UserRoles" ur
               INNER JOIN roles r ON r.id = ur."RoleId"
               WHERE ur."UserId" = @UserId
            """;

            using var connection = _sqlConnectionFactory.CreateConnection();
            var roles = await connection.QueryAsync<string>(
                sql,
                new { UserId = userId });

            var roleSet = roles.ToHashSet();

            await _cacheService.SetAsync(
                cacheKey,
                roleSet,
                TimeSpan.FromHours(1),
                cancellationToken);

            return roleSet;
        }


        private async Task<List<Claim>> CreateClaimsAsync(
            User user,
            HashSet<string> permissions,
            CancellationToken cancellationToken)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email.Value),
                new(JwtRegisteredClaimNames.Name, $"{user.Name.Value} {user.LastName.Value}"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.Value.ToString())
            };

            var roles = await GetUserRolesAsync(user.Id.Value, cancellationToken);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            foreach (var permission in permissions)
            {
                claims.Add(new Claim(CustomClaims.Permissions, permission));
            }

            return claims;
        }

        private string? GetTokenJti(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                return null;

            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }
    }
}
