using System.Diagnostics;

namespace ApiGateway.API.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = context.TraceIdentifier;
            var method = context.Request.Method;
            var path = context.Request.Path;
            var queryString = context.Request.QueryString;

            _logger.LogInformation(
                "Incoming Request: {Method} {Path}{QueryString} - TraceId: {TraceId}",
                method,
                path,
                queryString,
                requestId);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                var statusCode = context.Response.StatusCode;
                var logLevel = statusCode >= 500 ? LogLevel.Error :
                               statusCode >= 400 ? LogLevel.Warning :
                               LogLevel.Information;

                _logger.Log(
                    logLevel,
                    "Request Completed: {Method} {Path}{QueryString} - Status: {StatusCode} - Duration: {ElapsedMs}ms - TraceId: {TraceId}",
                    method,
                    path,
                    queryString,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    requestId);
            }
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
