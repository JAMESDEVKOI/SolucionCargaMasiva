using Auth.Domain.Interface;
using Auth.Domain.User.ValueObject;

namespace Auth.Domain.User.Events
{
    public record UserPasswordChangedDomainEvent(UserId UserId) : IDomainEvent;
}
