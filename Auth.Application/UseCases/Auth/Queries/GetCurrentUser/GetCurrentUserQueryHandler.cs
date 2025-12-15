using Auth.Application.Abstractions.Interfaces.Common;
using Auth.Application.Abstractions.Interfaces.Data;
using Auth.Application.Abstractions.Interfaces.Identity;
using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Primitives;
using Auth.Domain.User;
using Auth.Domain.User.ValueObject;
using Dapper;

namespace Auth.Application.UseCases.Auth.Queries.GetCurrentUser
{
    internal sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetCurrentUserQueryHandler(
            ICurrentUserService currentUserService,
            IUserRepository userRepository,
            ISqlConnectionFactory sqlConnectionFactory)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<CurrentUserResponse>> Handle(
            GetCurrentUserQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue || userId.Value == Guid.Empty)
            {
                return Result.Failure<CurrentUserResponse>(
                    new Error("Auth.Unauthorized", "Usuario no autenticado"));
            }

            var user = await _userRepository.GetByIdAsync(new UserId(userId.Value), cancellationToken);

            if (user is null)
            {
                return Result.Failure<CurrentUserResponse>(
                    new Error("Auth.UserNotFound", "Usuario no encontrado"));
            }

            var roles = await GetUserRolesAsync(userId.Value);
            var permissions = await GetUserPermissionsAsync(userId.Value);

            var response = new CurrentUserResponse(
                user.Id!.Value,
                user.Email.Value,
                user.Name.Value,
                user.LastName.Value,
                user.Phone?.Value ?? string.Empty,
                roles,
                permissions
            );

            return Result.Success(response);
        }

        private async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            const string sql = """
                SELECT DISTINCT r.name
                FROM "UserRoles" ur
                INNER JOIN roles r ON r.id = ur."RoleId"
                WHERE ur."UserId" = @UserId
            """;

            using var connection = _sqlConnectionFactory.CreateConnection();
            var roles = await connection.QueryAsync<string>(sql, new { UserId = userId });

            return roles ?? Enumerable.Empty<string>();
        }

        private async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
        {
            const string sql = """
                SELECT DISTINCT p.name
                FROM "UserRoles" ur
                INNER JOIN roles r ON r.id = ur."RoleId"
                INNER JOIN roles_permissions rp ON rp.role_id = r.id
                INNER JOIN permissions p ON p.id = rp.permission_id
                WHERE ur."UserId" = @UserId
            """;

            using var connection = _sqlConnectionFactory.CreateConnection();
            var permissions = await connection.QueryAsync<string>(sql, new { UserId = userId });

            return permissions ?? Enumerable.Empty<string>();
        }
    }
}
