using Auth.Application.DTOs.Users;

namespace Auth.Application.Abstractions.Interfaces.Repositories
{
    public interface IUserQueryRepository
    {
        Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserDto>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    }
}
