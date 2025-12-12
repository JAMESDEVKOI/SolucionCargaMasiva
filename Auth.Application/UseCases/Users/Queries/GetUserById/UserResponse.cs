using Auth.Application.DTOs.Users;

namespace Auth.Application.UseCases.Users.Queries.GetUserById
{
    public sealed record UserResponse(UserDto User);
}
