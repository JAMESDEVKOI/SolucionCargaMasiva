using Auth.Application.DTOs.Users;

namespace Auth.Application.UseCases.Users.Queries.GetAllUsers
{
    public sealed record UserListResponse(
        IEnumerable<UserDto> Users,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages);
}
