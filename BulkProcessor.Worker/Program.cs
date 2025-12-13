using BulkProcessor.Application;
using BulkProcessor.Infrastructure;
using BulkProcessor.Infrastructure.Persistence;
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

// Agregar Application y Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();

// Aplicar migraciones automáticamente al iniciar
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BulkProcessorDbContext>();

        Log.Information("Aplicando migraciones de base de datos...");
        await context.Database.MigrateAsync();
        Log.Information("Migraciones aplicadas exitosamente");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Error al aplicar migraciones de base de datos");
        throw;
    }
}

Log.Information("BulkProcessor Worker Service iniciando...");

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BulkProcessor Worker Service terminó inesperadamente");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
