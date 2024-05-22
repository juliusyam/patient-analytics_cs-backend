using System.Security.Claims;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using PatientAnalytics.Models.Auth;

namespace PatientAnalytics.Services;

public class PatientAnalyticsUserService
{
    private readonly AuthenticationDataMemoryStorage _authenticationDataMemoryStorage;
    private readonly JwtService _jwtService;
    private readonly ProtectedLocalStorage _protectedLocalStorage;

    public PatientAnalyticsUserService(
        AuthenticationDataMemoryStorage authenticationDataMemoryStorage,
        JwtService jwtService,
        ProtectedLocalStorage protectedLocalStorage)
    {
        _authenticationDataMemoryStorage = authenticationDataMemoryStorage;
        _jwtService = jwtService;
        _protectedLocalStorage = protectedLocalStorage;
    }

    public async Task<ClaimsPrincipal?> FetchBrowserTokenAsync()
    {
        var result = await _protectedLocalStorage.GetAsync<string>("user");
            
        if (result.Success && result.Value is not null)
        {
            _authenticationDataMemoryStorage.UpdateUser(result.Value, _jwtService.DecodeJwt(result.Value));
        }

        return _authenticationDataMemoryStorage.UserPrincipal;
    }

    public async Task<AuthenticationDataMemoryStorage> SendLoginRequestAsync(LoginResponse response)
    {
        _authenticationDataMemoryStorage.UpdateUser(response.Token, _jwtService.DecodeJwt(response.Token));
        await _protectedLocalStorage.SetAsync("user", response.Token);

        return _authenticationDataMemoryStorage;
    }

    public async Task SendLogoutRequestAsync()
    {
        _authenticationDataMemoryStorage.RemoveUser();
        await _protectedLocalStorage.DeleteAsync("user");
    }

    public AuthenticationDataMemoryStorage GetAuthenticationDataMemoryStorage()
    {
        return _authenticationDataMemoryStorage;
    }

    public void UpdateUserPrincipal(ClaimsPrincipal userPrincipal)
    {
        _authenticationDataMemoryStorage.UserPrincipal = userPrincipal;
    }
}