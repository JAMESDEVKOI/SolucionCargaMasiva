using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Interfaces;
using Notification.Domain.Interfaces;
using Notification.Infrastructure.Email;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Repositories;

namespace Notification.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            services.AddDbContext<NotificationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped<ICargaArchivoRepository, CargaArchivoRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Email
            services.Configure<SmtpSettings>(
                configuration.GetSection(nameof(SmtpSettings)));

            services.AddSingleton<IEmailSender, EmailSender>();

            // RabbitMQ Settings
            services.Configure<RabbitMQSettings>(
                configuration.GetSection(nameof(RabbitMQSettings)));

            // RabbitMQ Consumer (Background Service)
            services.AddHostedService<RabbitMQConsumer>();

            return services;
        }
    }
}
