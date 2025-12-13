using Notification.Application;
using Notification.Infrastructure;
using Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

// Registrar capas
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();

// Aplicar migraciones automáticamente
using (var scope = host.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await context.Database.MigrateAsync();
        Log.Information("Migraciones de Notification aplicadas correctamente");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error al aplicar migraciones de Notification");
        throw;
    }
}

Log.Information("Notification Worker iniciando...");

await host.RunAsync();
