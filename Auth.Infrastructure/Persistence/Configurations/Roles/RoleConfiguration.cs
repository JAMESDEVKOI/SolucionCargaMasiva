using Auth.Domain.Role;
using Auth.Domain.RolePermission;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations.Roles
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(r => r.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(r => r.Description)
                .HasColumnName("descripcion")
                .HasMaxLength(500);

            builder.HasIndex(r => r.Name)
                .IsUnique()
                .HasDatabaseName("ix_roles_nombre");

            builder.HasMany<RolePermission>()
               .WithOne()
               .HasForeignKey(rp => rp.RoleId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(
                Role.GetValues()
            );
        }
    }
}
