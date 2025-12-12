using Auth.Domain.Role;
using Auth.Domain.RolePermission;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations.Roles
{
    internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("roles_permissions");

            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Property(rp => rp.RoleId)
                .HasColumnName("role_id")
                .IsRequired();

            builder.Property(rp => rp.PermissionId)
                .HasColumnName("permission_id")
                .IsRequired();

            builder.HasOne<Role>()
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_roles_permissions_role_id");

            builder.HasOne<Domain.Permission.Permission>()
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_roles_permissions_permission_id");

            builder.HasIndex(rp => rp.RoleId)
                .HasDatabaseName("ix_roles_permissions_role_id");

            builder.HasIndex(rp => rp.PermissionId)
                .HasDatabaseName("ix_roles_permissions_permission_id");

            builder.Ignore(rp => rp.Id);

            var rolePermissions = new List<object>
            {
                // Client (1) - Solo puede leer su propia información
                new { RoleId = Role.Client.Id, PermissionId = Domain.Permission.Permission.ReadUser.Id },

                // Admin (2) - Gestión completa de usuarios y roles
                new { RoleId = Role.Admin.Id, PermissionId = Domain.Permission.Permission.ReadUser.Id },
                new { RoleId = Role.Admin.Id, PermissionId = Domain.Permission.Permission.WriteUser.Id },
                new { RoleId = Role.Admin.Id, PermissionId = Domain.Permission.Permission.UpdateUser.Id },
                new { RoleId = Role.Admin.Id, PermissionId = Domain.Permission.Permission.DeleteUser.Id },
                new { RoleId = Role.Admin.Id, PermissionId = Domain.Permission.Permission.ManageRoles.Id },
                new { RoleId = Role.Admin.Id, PermissionId = Domain.Permission.Permission.BulkUpload.Id },

                // Manager (3) - Permisos intermedios
                new { RoleId = Role.Manager.Id, PermissionId = Domain.Permission.Permission.ReadUser.Id },
                new { RoleId = Role.Manager.Id, PermissionId = Domain.Permission.Permission.UpdateUser.Id },
                new { RoleId = Role.Manager.Id, PermissionId = Domain.Permission.Permission.BulkUpload.Id },

                // SuperAdmin (4) - Todos los permisos
                new { RoleId = Role.SuperAdmin.Id, PermissionId = Domain.Permission.Permission.ReadUser.Id },
                new { RoleId = Role.SuperAdmin.Id, PermissionId = Domain.Permission.Permission.WriteUser.Id },
                new { RoleId = Role.SuperAdmin.Id, PermissionId = Domain.Permission.Permission.UpdateUser.Id },
                new { RoleId = Role.SuperAdmin.Id, PermissionId = Domain.Permission.Permission.DeleteUser.Id },
                new { RoleId = Role.SuperAdmin.Id, PermissionId = Domain.Permission.Permission.ManageRoles.Id },
                new { RoleId = Role.SuperAdmin.Id, PermissionId = Domain.Permission.Permission.BulkUpload.Id },
                new { RoleId = Role.SuperAdmin.Id, PermissionId = Domain.Permission.Permission.ManageSystem.Id }
            };

            builder.HasData(rolePermissions);
        }
    }
}
