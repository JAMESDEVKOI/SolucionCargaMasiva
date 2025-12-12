using Auth.API.Models;
using Auth.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace Auth.API.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                ValidationException validationException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Validation failed",
                    Errors = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .Select(g => new ErrorDetail
                        {
                            Field = g.Key,
                            Messages = g.Select(e => e.ErrorMessage).ToArray()
                        }).ToArray()
                },
                _ => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred",
                    Errors = new[]
                    {
                        new ErrorDetail
                        {
                            Field = "general",
                            Messages = new[] { exception.Message }
                        }
                    }
                }
            };

            context.Response.StatusCode = response.StatusCode;

            _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
