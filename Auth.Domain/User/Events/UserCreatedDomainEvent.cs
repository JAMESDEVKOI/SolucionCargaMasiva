using Auth.Domain.Interface;
using Auth.Domain.User.ValueObject;

namespace Auth.Domain.User.Events
{
    public sealed record UserCreatedDomainEvent(UserId UserId) : IDomainEvent;
}
