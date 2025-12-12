using Auth.Domain.Abstract;
using Auth.Domain.Interface;
using Auth.Domain.Primitives.Exceptions;
using Auth.Domain.User.ValueObject;

namespace Auth.Domain.UserRole
{
    public sealed class UserRole : Entity<Guid>, IAuditableEntity
    {
        private UserRole() { }

        public UserRole(UserId userId, int roleId)
            : base(Guid.NewGuid())
        {
            if (userId == null)
                throw new DomainException("El UserId no puede ser nulo");

            if (roleId <= 0)
                throw new DomainException("El RoleId debe ser mayor a 0");

            UserId = userId;
            RoleId = roleId;
            AssignedAt = DateTime.UtcNow;
        }

        public UserId UserId { get; private set; } = default!;
        public int RoleId { get; private set; }
        public DateTime AssignedAt { get; private set; }
        public User.User User { get; private set; } = default!;
        public Role.Role Role { get; private set; } = default!;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
