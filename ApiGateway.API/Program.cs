using ApiGateway.API.Extensions;
using ApiGateway.API.Middlewares;
using ApiGateway.API.RateLimiting;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ApiGateway")
    .WriteTo.Console()
    .WriteTo.File("logs/apigateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Ocelot configuration files
builder.Configuration.AddOcelotConfiguration(builder.Environment);

// Add services to the container
builder.Services.AddControllers();

// Add custom services
builder.Services.AddCustomCors(builder.Configuration);
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddCustomHealthChecks(builder.Configuration);
builder.Services.AddCustomSwagger();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add Ocelot with CacheManager
builder.Services.AddCustomOcelot(builder.Configuration);

// Add Rate Limiting configuration
builder.Services.AddRateLimitConfiguration(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline

// Global exception handler (must be first)
app.UseGlobalExceptionHandler();

// Request logging
app.UseRequestLogging();

// Convert access_token cookie to Authorization header (before Ocelot authentication)
app.UseCookieToHeaderMiddleware();

// Swagger (only in development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
        c.RoutePrefix = "swagger";
    });
}

// Response compression
app.UseResponseCompression();

// CORS
var corsPolicy = app.Environment.IsDevelopment() ? "AllowAll" : "AllowSpecificOrigins";
app.UseCors(corsPolicy);

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");

// Ocelot middleware (must be last)
await app.UseOcelot();

Log.Information("API Gateway started successfully on {Environment}", app.Environment.EnvironmentName);

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
