using Auth.Domain.User;
using Auth.Domain.User.ValueObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations.Users
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(
            EntityTypeBuilder<User> builder
            )
        {
            builder.ToTable("users");
            builder.HasKey(user => user.Id);

            builder.Property(user => user.Id)
            .HasConversion(userId => userId!.Value, value => new UserId(value));


            builder.Property(user => user.Name)
            .HasMaxLength(200)
            .HasConversion(nombre => nombre!.Value, value => new Name(value));


            builder.Property(user => user.LastName)
            .HasMaxLength(200)
            .HasConversion(apellido => apellido!.Value, value => new LastName(value));

            builder.Property(user => user.Email)
            .HasMaxLength(400)
            .HasConversion(email => email!.Value, value => new Email(value));

            builder.Property(user => user.Password)
            .HasMaxLength(2000)
            .HasConversion(password => password!.Value, value => new Password(value));

            builder.Property(user => user.Phone)
            .HasMaxLength(20)
            .HasConversion(
                phone => phone != null ? phone.Value : null,
                value => value != null ? new Phone(value) : null);

            builder.HasIndex(user => user.Email).IsUnique();

            builder.Property(user => user.CreatedAt)
                .IsRequired()
                .HasColumnName("CreatedAt");

            builder.Property(user => user.UpdatedAt)
                .HasColumnName("UpdatedAt");

            builder.Navigation(x => x.UserRoles)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Data - Usuarios de ejemplo
            var createdDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Password hash para todos: Admin123!, Manager123!, Client123!, SuperAdmin123!
            builder.HasData(
                new
                {
                    Id = new UserId(Guid.Parse("11111111-1111-1111-1111-111111111111")),
                    Name = new Name("Admin"),
                    LastName = new LastName("User"),
                    Email = new Email("admin@example.com"),
                    Password = new Password(BCrypt.Net.BCrypt.HashPassword("Admin123!")),
                    Phone = (Phone?)null,
                    CreatedAt = createdDate,
                    UpdatedAt = (DateTime?)null
                },
                new
                {
                    Id = new UserId(Guid.Parse("22222222-2222-2222-2222-222222222222")),
                    Name = new Name("Manager"),
                    LastName = new LastName("User"),
                    Email = new Email("manager@example.com"),
                    Password = new Password(BCrypt.Net.BCrypt.HashPassword("Manager123!")),
                    Phone = new Phone("+1234567890"),
                    CreatedAt = createdDate,
                    UpdatedAt = (DateTime?)null
                },
                new
                {
                    Id = new UserId(Guid.Parse("33333333-3333-3333-3333-333333333333")),
                    Name = new Name("Client"),
                    LastName = new LastName("User"),
                    Email = new Email("client@example.com"),
                    Password = new Password(BCrypt.Net.BCrypt.HashPassword("Client123!")),
                    Phone = new Phone("+9876543210"),
                    CreatedAt = createdDate,
                    UpdatedAt = (DateTime?)null
                },
                new
                {
                    Id = new UserId(Guid.Parse("44444444-4444-4444-4444-444444444444")),
                    Name = new Name("SuperAdmin"),
                    LastName = new LastName("User"),
                    Email = new Email("superadmin@example.com"),
                    Password = new Password(BCrypt.Net.BCrypt.HashPassword("SuperAdmin123!")),
                    Phone = new Phone("+1122334455"),
                    CreatedAt = createdDate,
                    UpdatedAt = (DateTime?)null
                }
            );
        }
    }
}
