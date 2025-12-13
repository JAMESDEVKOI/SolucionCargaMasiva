using BulkProcessor.Application.Interfaces;
using BulkProcessor.Domain.Interfaces;
using BulkProcessor.Infrastructure.Messaging;
using BulkProcessor.Infrastructure.Persistence;
using BulkProcessor.Infrastructure.Persistence.Repositories;
using BulkProcessor.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BulkProcessor.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<BulkProcessorDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(BulkProcessorDbContext).Assembly.FullName)));

            services.AddScoped<ICargaArchivoRepository, CargaArchivoRepository>();
            services.AddScoped<IDataProcesadaRepository, DataProcesadaRepository>();
            services.AddScoped<ICargaFalloRepository, CargaFalloRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IExcelParserService, ExcelParserService>();
            services.AddSingleton<IMessageBusService, RabbitMQPublisher>();

            services.Configure<SeaweedFSSettings>(
                configuration.GetSection(nameof(SeaweedFSSettings)));

            services.AddHttpClient<IFileStorageService, SeaweedFSService>();

            services.Configure<RabbitMQSettings>(
                configuration.GetSection(nameof(RabbitMQSettings)));

            services.AddHostedService<RabbitMQConsumer>();

            return services;
        }
    }
}
