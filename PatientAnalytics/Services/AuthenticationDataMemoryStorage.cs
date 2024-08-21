using System.Security.Claims;

namespace PatientAnalytics.Services;

public class AuthenticationDataMemoryStorage
{
    public string Token { get; private set; } = "";
    public string RefreshToken { get; private set; } = "";
    public ClaimsPrincipal? UserPrincipal { get; set; }

    public void UpdateClaimsPrincipal(ClaimsPrincipal? principal)
    {
        UserPrincipal = principal;
    }
    
    public void UpdateTokens(string token, string? refreshToken)
    {
        Token = token;
        RefreshToken = refreshToken ?? "";
    }

    public void RemoveUser()
    {
        Token = "";
        RefreshToken = "";
        UserPrincipal = null;
    }
}