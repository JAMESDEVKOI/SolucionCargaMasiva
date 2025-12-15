using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.UseCases.Auth.Queries.GetCurrentUser
{
    public sealed record GetCurrentUserQuery() : IQuery<CurrentUserResponse>;
}
