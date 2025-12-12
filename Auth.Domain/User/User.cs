using Auth.Domain.Abstract;
using Auth.Domain.Interface;
using Auth.Domain.Primitives;
using Auth.Domain.Primitives.Exceptions;
using Auth.Domain.User.Events;
using Auth.Domain.User.ValueObject;

namespace Auth.Domain.User
{
    public sealed class User : AggregateRoot<UserId>, IAuditableEntity
    {
        private readonly List<UserRole.UserRole> _userRoles = new();

        public Name Name { get; private set; } = default!;
        public LastName LastName { get; private set; } = default!;
        public Email Email { get; private set; } = default!;
        public Password Password { get; private set; } = default!;
        public Phone? Phone { get; private set; }

        public IReadOnlyCollection<UserRole.UserRole> UserRoles => _userRoles.AsReadOnly();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        private User() { }
        public User(UserId id, Name name, LastName lastName, Email email, Password password, Phone? phone)
            : base(id)
        {
            Name = name;
            LastName = lastName;
            Email = email;
            Password = password;
            Phone = phone;
        }

        public static User Create(
        Name name,
        LastName lastName,
        Email email,
        Password password,
        Phone? phone,
        int? defaultRoleId = null
        )
        {
            if (name is null)
                throw new DomainException("El nombre es requerido para crear un usuario");

            if (lastName is null)
                throw new DomainException("El apellido es requerido para crear un usuario");

            if (email is null)
                throw new DomainException("El email es requerido para crear un usuario");

            if (password is null)
                throw new DomainException("La contraseña es requerida para crear un usuario");

            var user = new User(UserId.New(), name, lastName, email, password, phone);

            var roleToAssign = defaultRoleId ?? Role.Role.Client.Id;

            var roleResult = user.AssignRole(roleToAssign);

            if (roleResult.IsFailure)
                throw new DomainException(roleResult.Error.Message);

            user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));
            return user;
        }
        public Result AssignRole(int roleId)
        {
            var role = Role.Role.FromValue(roleId);

            if (role is null)
                return Result.Failure(new Error("Role.NotFound", $"El rol con ID {roleId} no existe"));

            if (_userRoles.Any(r => r.RoleId == roleId))
                return Result.Failure(new Error("User.RoleAlreadyAssigned", $"El usuario ya tiene el rol '{role.Name}' asignado"));

            var userRole = new UserRole.UserRole(Id, roleId);

            _userRoles.Add(userRole);

            RaiseDomainEvent(new UserRoleAssignedDomainEvent(Id, roleId));

            return Result.Success();
        }

        public Result UpdateProfile(Name name, LastName lastName, Phone? phone)
        {
            if (name is null)
                return Result.Failure(new Error("User.InvalidName", "El nombre no puede ser nulo"));

            if (lastName is null)
                return Result.Failure(new Error("User.InvalidLastName", "El apellido no puede ser nulo"));

            Name = name;
            LastName = lastName;
            Phone = phone;

            return Result.Success();
        }

        public Result RemoveRole(int roleId)
        {
            var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);

            if (userRole is null)
                return Result.Failure(new Error("User.RoleNotAssigned", "El usuario no tiene ese rol asignado"));

            if (_userRoles.Count == 1)
                return Result.Failure(new Error("User.MustHaveRole", "El usuario debe tener al menos un rol"));

            _userRoles.Remove(userRole);

            RaiseDomainEvent(new UserRoleRemovedDomainEvent(Id, roleId));

            return Result.Success();
        }
    }
}
