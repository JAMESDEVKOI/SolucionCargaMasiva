using Auth.Domain.Interface;
using Auth.Domain.User.ValueObject;

namespace Auth.Domain.User.Events
{
    public record UserRoleRemovedDomainEvent( UserId UserId, int RoleId) : IDomainEvent;
}
