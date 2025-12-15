namespace FileControl.API.Middleware
{
    /// <summary>
    /// Middleware que convierte la cookie access_token en un header Authorization
    /// para que la autenticación JWT pueda funcionar con cookies
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
            // Si no hay header Authorization pero hay cookie access_token,
            // copiar el token de la cookie al header Authorization
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                var accessToken = context.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogDebug("Converting access_token cookie to Authorization header");
                    context.Request.Headers.Append("Authorization", $"Bearer {accessToken}");
                }
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
