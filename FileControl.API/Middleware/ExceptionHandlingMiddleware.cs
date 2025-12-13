using FluentValidation;
using System.Net;
using System.Text.Json;

namespace FileControl.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var (statusCode, message, errors) = exception switch
            {
                ValidationException validationException => (
                    HttpStatusCode.BadRequest,
                    "Errores de validación",
                    validationException.Errors.Select(e => new
                    {
                        property = e.PropertyName,
                        error = e.ErrorMessage
                    }).ToList()
                ),
                UnauthorizedAccessException => (
                    HttpStatusCode.Forbidden,
                    "No tiene permisos para realizar esta operación",
                    null
                ),
                InvalidOperationException => (
                    HttpStatusCode.BadRequest,
                    exception.Message,
                    null
                ),
                _ => (
                    HttpStatusCode.InternalServerError,
                    "Ocurrió un error interno en el servidor",
                    null
                )
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                statusCode = (int)statusCode,
                message,
                errors
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
