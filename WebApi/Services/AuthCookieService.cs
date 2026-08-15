using Application.Interfaces;

namespace WebApi.Services;

public class AuthCookieService(IHttpContextAccessor httpContextAccessor) : IAuthCookieService
{
    private const string AccessTokenCookieName = "accessToken";
    private const string RefreshTokenCookieName = "refreshToken";
    private const string RefreshTokenCookiePath = "/api/auth";

    public void SetTokenCookies(string accessToken, DateTime accessTokenExpiresAt, string refreshToken)
    {
        var response = GetHttpContext().Response;

        response.Cookies.Append(AccessTokenCookieName, accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = accessTokenExpiresAt
        });

        response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = RefreshTokenCookiePath,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    public void ClearTokenCookies()
    {
        var response = GetHttpContext().Response;

        response.Cookies.Delete(AccessTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        });

        response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = RefreshTokenCookiePath
        });
    }

    public string? GetRefreshToken() => GetHttpContext().Request.Cookies[RefreshTokenCookieName];

    private HttpContext GetHttpContext() =>
        httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available.");
}
