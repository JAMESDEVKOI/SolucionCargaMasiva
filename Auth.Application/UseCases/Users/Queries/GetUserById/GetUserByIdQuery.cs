using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.UseCases.Users.Queries.GetUserById
{
    public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserResponse>;
}
