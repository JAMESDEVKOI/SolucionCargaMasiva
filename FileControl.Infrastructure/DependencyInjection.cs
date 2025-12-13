using FileControl.Application.Interfaces;
using FileControl.Domain.Interfaces;
using FileControl.Infrastructure.Messaging;
using FileControl.Infrastructure.Persistence;
using FileControl.Infrastructure.Persistence.Repositories;
using FileControl.Infrastructure.Services;
using FileControl.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileControl.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddPersistence(configuration);
            services.AddStorage(configuration);
            services.AddMessaging(configuration);
            services.AddServices();

            return services;
        }

        private static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Database")
                ?? throw new InvalidOperationException("Database connection string is not configured");

            services.AddDbContext<FileControlDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FileControlDbContext>());
            services.AddScoped<ICargaArchivoRepository, CargaArchivoRepository>();

            return services;
        }

        private static IServiceCollection AddStorage(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<SeaweedFSSettings>(
                configuration.GetSection(SeaweedFSSettings.SectionName));

            services.AddHttpClient<IFileStorageService, SeaweedFSService>();

            return services;
        }

        private static IServiceCollection AddMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<RabbitMQSettings>(
                configuration.GetSection(RabbitMQSettings.SectionName));

            services.AddSingleton<IMessageBusService, RabbitMQService>();

            return services;
        }

        private static IServiceCollection AddServices(
            this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }
}
