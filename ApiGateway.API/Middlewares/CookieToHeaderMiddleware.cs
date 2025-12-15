namespace ApiGateway.API.Middlewares
{
    /// <summary>
    /// Middleware que convierte la cookie access_token en un header Authorization
    /// para que Ocelot pueda autenticar las peticiones
    /// </summary>
    public class CookieToHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CookieToHeaderMiddleware> _logger;

        public CookieToHeaderMiddleware(RequestDelegate next, ILogger<CookieToHeaderMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("CookieToHeaderMiddleware executing for path: {Path}", context.Request.Path);

            // Log all cookies
            var cookieNames = string.Join(", ", context.Request.Cookies.Keys);
            _logger.LogInformation("Available cookies: {Cookies}", string.IsNullOrEmpty(cookieNames) ? "NONE" : cookieNames);

            // Si no hay header Authorization pero hay cookie access_token,
            // copiar el token de la cookie al header Authorization
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                var accessToken = context.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogInformation("Converting access_token cookie to Authorization header (token length: {Length})", accessToken.Length);
                    context.Request.Headers.Append("Authorization", $"Bearer {accessToken}");

                    // Verify the header was set
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    _logger.LogInformation("Authorization header after setting: {Header}",
                        authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader);
                }
                else
                {
                    _logger.LogWarning("No access_token cookie found!");
                }
            }
            else
            {
                var existingHeader = context.Request.Headers["Authorization"].ToString();
                _logger.LogInformation("Authorization header already present: {Header}",
                    existingHeader.Length > 50 ? existingHeader.Substring(0, 50) + "..." : existingHeader);
            }

            await _next(context);
        }
    }

    public static class CookieToHeaderMiddlewareExtensions
    {
        public static IApplicationBuilder UseCookieToHeaderMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CookieToHeaderMiddleware>();
        }
    }
}
