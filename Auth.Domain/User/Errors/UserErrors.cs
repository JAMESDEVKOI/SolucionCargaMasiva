using Auth.Domain.Primitives;

namespace Auth.Domain.User.Errors
{
    public static class UserErrors
    {
        public static Error NotFound(Guid userId) => new(
            "User.NotFound",
            $"User with ID {userId} was not found");

        public static Error EmailAlreadyExists => new(
            "User.EmailAlreadyExists",
            "A user with this email already exists");

        public static Error UserNameAlreadyExists => new(
            "User.UserNameAlreadyExists",
            "A user with this username already exists");

        public static Error AlreadyConfirmed => new(
            "User.EmailAlreadyConfirmed",
            "Email is already confirmed");

        public static Error LockedOut => new(
            "User.LockedOut",
            "User account is locked out");

        public static Error InvalidCredentials => new(
            "User.InvalidCredentials",
            "Invalid email or password");
    }
}
