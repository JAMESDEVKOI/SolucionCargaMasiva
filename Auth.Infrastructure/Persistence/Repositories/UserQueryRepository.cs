using Auth.Application.Abstractions.Interfaces.Data;
using Auth.Application.Abstractions.Interfaces.Repositories;
using Auth.Application.DTOs.Users;
using Dapper;

namespace Auth.Infrastructure.Persistence.Repositories
{
    internal sealed class UserQueryRepository : IUserQueryRepository
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public UserQueryRepository(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = @"
                SELECT
                    u.""Id"",
                    u.""Name"",
                    u.""LastName"",
                    u.""Email"",
                    u.""Phone"",
                    u.""CreatedOnUtc"" AS CreatedAt,
                    u.""ModifiedOnUtc"" AS UpdatedAt,
                    ARRAY_AGG(r.""Name"") FILTER (WHERE r.""Name"" IS NOT NULL) AS Roles
                FROM ""Users"" u
                LEFT JOIN ""UserRoles"" ur ON u.""Id"" = ur.""UserId""
                LEFT JOIN ""Roles"" r ON ur.""RoleId"" = r.""Id""
                WHERE u.""Id"" = @UserId
                GROUP BY u.""Id"", u.""Name"", u.""LastName"", u.""Email"", u.""Phone"", u.""CreatedOnUtc"", u.""ModifiedOnUtc""";

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                sql,
                new { UserId = id });

            if (result == null)
                return null;

            return new UserDto(
                result.Id,
                result.Name,
                result.LastName,
                result.Email,
                result.Phone,
                result.CreatedAt,
                result.UpdatedAt,
                result.Roles ?? Array.Empty<string>());
        }

        public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = @"
                SELECT
                    u.""Id"",
                    u.""Name"",
                    u.""LastName"",
                    u.""Email"",
                    u.""Phone"",
                    u.""CreatedOnUtc"" AS CreatedAt,
                    u.""ModifiedOnUtc"" AS UpdatedAt,
                    ARRAY_AGG(r.""Name"") FILTER (WHERE r.""Name"" IS NOT NULL) AS Roles
                FROM ""Users"" u
                LEFT JOIN ""UserRoles"" ur ON u.""Id"" = ur.""UserId""
                LEFT JOIN ""Roles"" r ON ur.""RoleId"" = r.""Id""
                WHERE u.""Email"" = @Email
                GROUP BY u.""Id"", u.""Name"", u.""LastName"", u.""Email"", u.""Phone"", u.""CreatedOnUtc"", u.""ModifiedOnUtc""";

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                sql,
                new { Email = email });

            if (result == null)
                return null;

            return new UserDto(
                result.Id,
                result.Name,
                result.LastName,
                result.Email,
                result.Phone,
                result.CreatedAt,
                result.UpdatedAt,
                result.Roles ?? Array.Empty<string>());
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = @"
                SELECT
                    u.""Id"",
                    u.""Name"",
                    u.""LastName"",
                    u.""Email"",
                    u.""Phone"",
                    u.""CreatedOnUtc"" AS CreatedAt,
                    u.""ModifiedOnUtc"" AS UpdatedAt,
                    ARRAY_AGG(r.""Name"") FILTER (WHERE r.""Name"" IS NOT NULL) AS Roles
                FROM ""Users"" u
                LEFT JOIN ""UserRoles"" ur ON u.""Id"" = ur.""UserId""
                LEFT JOIN ""Roles"" r ON ur.""RoleId"" = r.""Id""
                GROUP BY u.""Id"", u.""Name"", u.""LastName"", u.""Email"", u.""Phone"", u.""CreatedOnUtc"", u.""ModifiedOnUtc""
                ORDER BY u.""CreatedOnUtc"" DESC
                LIMIT @PageSize OFFSET @Offset";

            var results = await connection.QueryAsync<dynamic>(
                sql,
                new { PageSize = pageSize, Offset = (page - 1) * pageSize });

            return results.Select(r => new UserDto(
                r.Id,
                r.Name,
                r.LastName,
                r.Email,
                r.Phone,
                r.CreatedAt,
                r.UpdatedAt,
                r.Roles ?? Array.Empty<string>()));
        }

        public async Task<IEnumerable<UserDto>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = @"
                SELECT
                    u.""Id"",
                    u.""Name"",
                    u.""LastName"",
                    u.""Email"",
                    u.""Phone"",
                    u.""CreatedOnUtc"" AS CreatedAt,
                    u.""ModifiedOnUtc"" AS UpdatedAt,
                    ARRAY_AGG(r2.""Name"") FILTER (WHERE r2.""Name"" IS NOT NULL) AS Roles
                FROM ""Users"" u
                INNER JOIN ""UserRoles"" ur ON u.""Id"" = ur.""UserId""
                INNER JOIN ""Roles"" r ON ur.""RoleId"" = r.""Id""
                LEFT JOIN ""UserRoles"" ur2 ON u.""Id"" = ur2.""UserId""
                LEFT JOIN ""Roles"" r2 ON ur2.""RoleId"" = r2.""Id""
                WHERE r.""Name"" = @RoleName
                GROUP BY u.""Id"", u.""Name"", u.""LastName"", u.""Email"", u.""Phone"", u.""CreatedOnUtc"", u.""ModifiedOnUtc""";

            var results = await connection.QueryAsync<dynamic>(
                sql,
                new { RoleName = roleName });

            return results.Select(r => new UserDto(
                r.Id,
                r.Name,
                r.LastName,
                r.Email,
                r.Phone,
                r.CreatedAt,
                r.UpdatedAt,
                r.Roles ?? Array.Empty<string>()));
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = @"
                SELECT r.""Name""
                FROM ""Roles"" r
                INNER JOIN ""UserRoles"" ur ON r.""Id"" = ur.""RoleId""
                WHERE ur.""UserId"" = @UserId";

            return await connection.QueryAsync<string>(sql, new { UserId = userId });
        }

        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();
            const string sql = @"SELECT COUNT(*) FROM ""Users""";
            return await connection.ExecuteScalarAsync<int>(sql);
        }
    }
}
