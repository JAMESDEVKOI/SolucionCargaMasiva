using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations.Permission
{
    internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Domain.Permission.Permission>
    {
        public void Configure(EntityTypeBuilder<Domain.Permission.Permission> builder)
        {
            builder.ToTable("permissions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(p => p.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.HasIndex(p => p.Name)
                .IsUnique()
                .HasDatabaseName("ix_permissions_name");

            builder.HasData(
                Domain.Permission.Permission.GetValues()
            );
        }
    }
}
