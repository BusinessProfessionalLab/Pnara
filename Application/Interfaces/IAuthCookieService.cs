namespace Application.Interfaces;

public interface IAuthCookieService
{
    void SetTokenCookies(string accessToken, DateTime accessTokenExpiresAt, string refreshToken);
    void ClearTokenCookies();
    string? GetRefreshToken();
}
