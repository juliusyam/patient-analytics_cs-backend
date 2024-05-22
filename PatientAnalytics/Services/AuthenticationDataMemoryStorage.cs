using System.Security.Claims;
using PatientAnalytics.Services;

namespace PatientAnalytics.Services;

public class AuthenticationDataMemoryStorage
{
    public string Token { get; private set; } = "";
    public ClaimsPrincipal? UserPrincipal { get; set; } = null;

    public void UpdateUser(string token, ClaimsPrincipal? principal)
    {
        Token = token;
        UserPrincipal = principal;
    }

    public void RemoveUser()
    {
        Token = "";
        UserPrincipal = null;
    }
}