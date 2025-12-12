namespace Auth.Application.Abstractions.Interfaces.Identity
{
    public interface ICookieAuthenticationProvider
    {
        void SetAccessTokenCookie(string accessToken);

        void SetRefreshTokenCookie(string refreshToken);

        void SetAuthenticationCookies(string accessToken, string refreshToken);

        string? GetAccessToken();

        string? GetRefreshToken();

        void RemoveAuthenticationCookies();

        bool HasAccessToken();

        bool HasRefreshToken();
    }
}
