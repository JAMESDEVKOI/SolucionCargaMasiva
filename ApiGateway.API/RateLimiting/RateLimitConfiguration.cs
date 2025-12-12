namespace ApiGateway.API.RateLimiting
{
    public class RateLimitConfiguration
    {
        public const string SectionName = "RateLimiting";

        public bool EnableRateLimiting { get; set; } = true;
        public List<RateLimitPolicy> Policies { get; set; } = new();
        public GlobalRateLimitOptions GlobalOptions { get; set; } = new();
    }

    public class RateLimitPolicy
    {
        public string Name { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public int PermitLimit { get; set; }
        public int Window { get; set; }
        public string WindowUnit { get; set; } = "seconds";
        public int QueueLimit { get; set; }
    }

    public class GlobalRateLimitOptions
    {
        public bool DisableRateLimitHeaders { get; set; } = false;
        public string QuotaExceededMessage { get; set; } = "Rate limit exceeded. Please try again later.";
        public int HttpStatusCode { get; set; } = 429;
        public string ClientIdHeader { get; set; } = "X-ClientId";
        public List<string> ClientWhitelist { get; set; } = new();
        public List<string> IpWhitelist { get; set; } = new();
    }

    public static class RateLimitConfigurationExtensions
    {
        public static IServiceCollection AddRateLimitConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<RateLimitConfiguration>(
                configuration.GetSection(RateLimitConfiguration.SectionName));

            return services;
        }
    }
}
