using Auth.Domain.Role;
using Auth.Domain.User.ValueObject;
using Auth.Domain.UserRole;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations.Users
{
    public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .HasConversion(userId => userId!.Value, value => new UserId(value))
                .HasColumnName("UserId");

            builder.Property(x => x.RoleId)
                .HasColumnName("RoleId")
                .IsRequired();

            builder.Property(x => x.AssignedAt)
                .HasColumnName("AssignedAt")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");

            builder.Ignore(x => x.User);

            builder.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_UserRoles_Roles_RoleId");

            builder.HasIndex(x => new { x.UserId, x.RoleId })
                .IsUnique();

            // Seed Data - Asignación de roles a usuarios
            var assignedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.HasData(
                new
                {
                    Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                    UserId = new UserId(Guid.Parse("11111111-1111-1111-1111-111111111111")),
                    RoleId = Role.Admin.Id,
                    AssignedAt = assignedDate,
                    CreatedAt = assignedDate,
                    UpdatedAt = (DateTime?)null
                },
                new
                {
                    Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                    UserId = new UserId(Guid.Parse("22222222-2222-2222-2222-222222222222")),
                    RoleId = Role.Manager.Id,
                    AssignedAt = assignedDate,
                    CreatedAt = assignedDate,
                    UpdatedAt = (DateTime?)null
                },
                new
                {
                    Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                    UserId = new UserId(Guid.Parse("33333333-3333-3333-3333-333333333333")),
                    RoleId = Role.Client.Id,
                    AssignedAt = assignedDate,
                    CreatedAt = assignedDate,
                    UpdatedAt = (DateTime?)null
                },
                new
                {
                    Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"),
                    UserId = new UserId(Guid.Parse("44444444-4444-4444-4444-444444444444")),
                    RoleId = Role.SuperAdmin.Id,
                    AssignedAt = assignedDate,
                    CreatedAt = assignedDate,
                    UpdatedAt = (DateTime?)null
                }
            );
        }
    }
}
