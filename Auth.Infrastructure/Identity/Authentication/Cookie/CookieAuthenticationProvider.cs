using Auth.Application.Abstractions.Interfaces.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.Identity.Authentication.Cookie
{
    internal class CookieAuthenticationProvider : ICookieAuthenticationProvider
    {
        private readonly CookieSettings _settings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public CookieAuthenticationProvider(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _settings = CookieSettings.LoadFromConfiguration(configuration);
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public void SetAccessTokenCookie(string accessToken)
        {
            var httpContext = GetHttpContext();
            var expirationMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 60);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = _settings.HttpOnly,
                Secure = _settings.Secure,
                SameSite = ParseSameSiteMode(_settings.SameSite),
                Path = _settings.Path,
                Domain = _settings.Domain,
                Expires = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
            };

            httpContext.Response.Cookies.Append(_settings.AccessTokenCookieName, accessToken, cookieOptions);
        }

        public void SetRefreshTokenCookie(string refreshToken)
        {
            var httpContext = GetHttpContext();
            var expirationDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth/refresh",
                Domain = _settings.Domain,
                Expires = DateTimeOffset.UtcNow.AddDays(expirationDays)
            };

            httpContext.Response.Cookies.Append(_settings.RefreshTokenCookieName, refreshToken, cookieOptions);
        }

        public void SetAuthenticationCookies(string accessToken, string refreshToken)
        {
            SetAccessTokenCookie(accessToken);
            SetRefreshTokenCookie(refreshToken);
        }

        public string? GetAccessToken()
        {
            var httpContext = GetHttpContext();
            return httpContext.Request.Cookies[_settings.AccessTokenCookieName];
        }

        public string? GetRefreshToken()
        {
            var httpContext = GetHttpContext();
            return httpContext.Request.Cookies[_settings.RefreshTokenCookieName];
        }

        public void RemoveAuthenticationCookies()
        {
            var httpContext = GetHttpContext();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = _settings.Path,
                Domain = _settings.Domain,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            };

            httpContext.Response.Cookies.Delete(_settings.AccessTokenCookieName, cookieOptions);

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth/refresh",
                Domain = _settings.Domain,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            };

            httpContext.Response.Cookies.Delete(_settings.RefreshTokenCookieName, refreshCookieOptions);
        }

        public bool HasAccessToken()
        {
            return !string.IsNullOrEmpty(GetAccessToken());
        }

        public bool HasRefreshToken()
        {
            return !string.IsNullOrEmpty(GetRefreshToken());
        }

        private HttpContext GetHttpContext()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new InvalidOperationException("HttpContext is not available");
            return httpContext;
        }

        private static SameSiteMode ParseSameSiteMode(string mode)
        {
            return mode.ToLower() switch
            {
                "strict" => SameSiteMode.Strict,
                "lax" => SameSiteMode.Lax,
                "none" => SameSiteMode.None,
                _ => SameSiteMode.Strict
            };
        }
    }
}
