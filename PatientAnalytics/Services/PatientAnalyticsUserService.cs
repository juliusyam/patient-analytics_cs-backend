using System.Security.Claims;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using PatientAnalytics.Middleware;
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

    public async Task<ClaimsPrincipal?> FetchUserFromStorageAsync(Func<Task> refreshCallback)
    {
        var result = await _protectedLocalStorage.GetAsync<string>("user");
        var refreshResult = await _protectedLocalStorage.GetAsync<string>("user-refresh");
            
        if (result.Success && result.Value is not null)
        {
            _authenticationDataMemoryStorage.UpdateTokens(result.Value, refreshResult.Value);
            
            try
            {
                // TODO: Check if is deactivated
                _authenticationDataMemoryStorage.UpdateClaimsPrincipal(_jwtService.DecodeJwt(result.Value));
            }
            catch (HttpStatusCodeException exception)
            {
                if (exception.StatusCode == 401)
                {
                   await refreshCallback();
                }
            }
        }

        return _authenticationDataMemoryStorage.UserPrincipal;
    }

    public async Task<AuthenticationDataMemoryStorage> SaveUserInStorageAsync(LoginResponse response)
    {
        _authenticationDataMemoryStorage.UpdateTokens(response.Token, response.RefreshToken);
        _authenticationDataMemoryStorage.UpdateClaimsPrincipal(_jwtService.DecodeJwt(response.Token));
        
        await _protectedLocalStorage.SetAsync("user", response.Token);
        await _protectedLocalStorage.SetAsync("user-refresh", response.RefreshToken);

        return _authenticationDataMemoryStorage;
    }

    public async Task RemoveUserFromStorageAsync()
    {
        _authenticationDataMemoryStorage.RemoveUser();
        await _protectedLocalStorage.DeleteAsync("user");
        await _protectedLocalStorage.DeleteAsync("user-refresh");
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