using Auth.Domain.Primitives;

namespace Auth.Domain.Permission
{
    public sealed class Permission : Enumeration<Permission>
    {

        public static readonly Permission ReadUser = new(1, "ReadUser", "Leer usuarios");
        public static readonly Permission WriteUser = new(2, "WriteUser", "Crear usuarios");
        public static readonly Permission UpdateUser = new(3, "UpdateUser", "Actualizar usuarios");
        public static readonly Permission DeleteUser = new(4, "DeleteUser", "Eliminar usuarios");
        public static readonly Permission ManageRoles = new(5, "ManageRoles", "Gestionar roles");
        public static readonly Permission ManageSystem = new(6, "ManageSystem", "Administrar sistema");
        public static readonly Permission BulkUpload = new(7, "BulkUpload", "Hacer la carga masiva");

        private Permission(int id, string name, string description)
            : base(id, name)
        {
            Description = description;
        }

        public string Description { get; }
    }
}
