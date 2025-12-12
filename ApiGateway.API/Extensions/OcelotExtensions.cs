using Ocelot.DependencyInjection;
using Ocelot.Cache.CacheManager;
using Ocelot.Provider.Polly;

namespace ApiGateway.API.Extensions
{
    public static class OcelotExtensions
    {
        public static IServiceCollection AddCustomOcelot(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOcelot(configuration)
                    .AddCacheManager(x =>
                    {
                        x.WithDictionaryHandle();
                    })
                    .AddPolly();

            return services;
        }

        public static IConfigurationBuilder AddOcelotConfiguration(this IConfigurationBuilder builder, IWebHostEnvironment environment)
        {
            builder.AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true)
                   .AddJsonFile($"Configuration/ocelot.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            return builder;
        }
    }
}
