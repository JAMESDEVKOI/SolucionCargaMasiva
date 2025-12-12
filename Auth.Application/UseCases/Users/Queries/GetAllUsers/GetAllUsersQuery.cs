using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.UseCases.Users.Queries.GetAllUsers
{
    public sealed record GetAllUsersQuery(
           int Page = 1,
           int PageSize = 10) : IQuery<UserListResponse>;
}
