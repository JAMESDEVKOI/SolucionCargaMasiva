using Auth.Domain.Primitives;

namespace Auth.Domain.Role
{
    public sealed class Role : Enumeration<Role>
    {
        public static readonly Role Client = new(1, "Client", "Usuario cliente estándar");
        public static readonly Role Admin = new(2, "Admin", "Administrador del sistema");
        public static readonly Role Manager = new(3, "Manager", "Gerente con permisos especiales");
        public static readonly Role SuperAdmin = new(4, "SuperAdmin", "Super administrador");

        private Role(int id, string name, string description)
            : base(id, name)
        {
            Description = description;
        }

        public string Description { get; }

        public bool IsAdmin() => this == Admin || this == SuperAdmin;
        public bool CanManageUsers() => this == Admin || this == SuperAdmin || this == Manager;
    }
}
