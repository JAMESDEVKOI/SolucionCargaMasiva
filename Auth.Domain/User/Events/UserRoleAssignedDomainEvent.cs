using Auth.Domain.Interface;
using Auth.Domain.User.ValueObject;

namespace Auth.Domain.User.Events
{
    public sealed record UserRoleAssignedDomainEvent(UserId UserId, int RoleId) : IDomainEvent;
}
