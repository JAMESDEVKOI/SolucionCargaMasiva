using Auth.Domain.Abstract;
using Auth.Domain.Primitives.Exceptions;

namespace Auth.Domain.RolePermission
{
    public sealed class RolePermission : Entity<Guid>
    {
        private RolePermission() { }

        public RolePermission(int roleId, int permissionId)
            : base(Guid.NewGuid())
        {
            if (roleId <= 0)
                throw new DomainException("El RoleId debe ser mayor a 0");

            if (permissionId <= 0)
                throw new DomainException("El PermissionId debe ser mayor a 0");

            RoleId = roleId;
            PermissionId = permissionId;
        }

        public int RoleId { get; private set; }
        public int PermissionId { get; private set; }

    }
}
