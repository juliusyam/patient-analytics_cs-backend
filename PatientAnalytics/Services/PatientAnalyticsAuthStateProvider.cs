using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using PatientAnalytics.Models.Auth;

namespace PatientAnalytics.Services;

public class PatientAnalyticsAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly PatientAnalyticsUserService _patientAnalyticsUserService;

    public PatientAnalyticsAuthStateProvider(PatientAnalyticsUserService patientAnalyticsUserService)
    {
        _patientAnalyticsUserService = patientAnalyticsUserService;
        AuthenticationStateChanged += OnAuthenticationStateChangedAsync;
    }

    public async Task OnAfterRenderAsync()
    {
        var principal = new ClaimsPrincipal();
        var userPrincipal = await _patientAnalyticsUserService.FetchBrowserTokenAsync();

        if (userPrincipal is not null)
        {
            principal = userPrincipal;
        };
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }
    
    public async Task Login(LoginResponse response)
    {
        var principal = new ClaimsPrincipal();
        var updatedUser = await _patientAnalyticsUserService.SendLoginRequestAsync(response);
        
        if (updatedUser.UserPrincipal is not null)
        {
            principal = updatedUser.UserPrincipal;
        };
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task Logout()
    {
        await _patientAnalyticsUserService.SendLogoutRequestAsync();
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
    }
    
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = new ClaimsPrincipal();
        var memoryStorage = _patientAnalyticsUserService.GetAuthenticationDataMemoryStorage();

        if (memoryStorage.UserPrincipal is not null)
        {
            principal = memoryStorage.UserPrincipal;
        };
        
        return Task.FromResult(new AuthenticationState(principal));
    }

    private async void OnAuthenticationStateChangedAsync(Task<AuthenticationState> task)
    {
        var authenticationState = await task;
        _patientAnalyticsUserService.UpdateUserPrincipal(authenticationState.User);
    }

    public AuthenticationDataMemoryStorage FetchCurrentUser()
    {
        return _patientAnalyticsUserService.GetAuthenticationDataMemoryStorage();
    }

    public bool HasAdminPrivileges()
    {
        var userPrincipal = _patientAnalyticsUserService.GetAuthenticationDataMemoryStorage().UserPrincipal;

        return userPrincipal is not null && (userPrincipal.HasClaim(ClaimTypes.Role, "SuperAdmin") || userPrincipal.HasClaim(ClaimTypes.Role, "Admin"));
    }
    
    public bool IsDoctor()
    {
        var userPrincipal = _patientAnalyticsUserService.GetAuthenticationDataMemoryStorage().UserPrincipal;

        return userPrincipal is not null && userPrincipal.HasClaim(ClaimTypes.Role, "Doctor");
    }
    
    
    public bool IsSuperAdmin()
    {
        var userPrincipal = _patientAnalyticsUserService.GetAuthenticationDataMemoryStorage().UserPrincipal;

        return userPrincipal is not null && userPrincipal.HasClaim(ClaimTypes.Role, "SuperAdmin");
    }

    public void Dispose()
    {
        AuthenticationStateChanged -= OnAuthenticationStateChangedAsync;
    }
}