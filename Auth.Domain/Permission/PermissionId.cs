namespace Auth.Domain.Permission
{
    public record PermissionId(int Value)
    {
        public static PermissionId From(int value) => new(value);
    }
}
