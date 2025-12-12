using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.Identity.Authentication.Cookie
{
    internal class CookieSettings
    {
        public string AccessTokenCookieName { get; set; } = "access_token";
        public string RefreshTokenCookieName { get; set; } = "refresh_token";
        public bool HttpOnly { get; set; } = true;
        public bool Secure { get; set; } = true;
        public string SameSite { get; set; } = "Strict";
        public string Path { get; set; } = "/";
        public string? Domain { get; set; }

        public static CookieSettings LoadFromConfiguration(IConfiguration configuration)
        {
            var settings = new CookieSettings();
            configuration.GetSection("Authentication:Cookie").Bind(settings);
            return settings;
        }
    }
}
