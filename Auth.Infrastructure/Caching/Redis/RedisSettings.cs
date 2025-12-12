namespace Auth.Infrastructure.Caching.Redis
{
    public sealed class RedisSettings
    {
        public const string SectionName = "RedisSettings";
        public string ConnectionString { get; init; } = string.Empty;
        public int DefaultExpirationMinutes { get; init; } = 60;
        public string? InstanceName { get; init; }
    }
}
