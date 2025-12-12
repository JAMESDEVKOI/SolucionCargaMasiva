namespace Auth.Domain.User.ValueObject
{
    public record UserId(Guid Value)
    {
        public static UserId New() => new(Guid.NewGuid());
    }
}
