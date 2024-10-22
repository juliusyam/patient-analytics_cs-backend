using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;

namespace PatientAnalytics.Services;

public class PatientAnalyticsAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly AuthService _authService;
    private readonly PatientAnalyticsUserService _patientAnalyticsUserService;
    private readonly IConfiguration _config;
    private HubConnection? _hubConnection;

    public PatientAnalyticsAuthStateProvider(
        PatientAnalyticsUserService patientAnalyticsUserService,
        AuthService authService,
        IConfiguration config)
    {
        _patientAnalyticsUserService = patientAnalyticsUserService;
        _authService = authService;
        _config = config;
        AuthenticationStateChanged += OnAuthenticationStateChangedAsync;
    }

    public async Task OnAfterRenderAsync()
    {
        var principal = new ClaimsPrincipal();

        var userPrincipal = await _patientAnalyticsUserService.FetchUserFromStorageAsync(RefreshAsync);

        if (userPrincipal is not null)
        {
            principal = userPrincipal;
        }
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }
    
    public async Task<LoginResponse> LoginAsync(LoginPayload loginPayload)
    {
        var response = await _authService.Login(loginPayload);
        
        var principal = new ClaimsPrincipal();

        var updatedUser = await _patientAnalyticsUserService.SaveUserInStorageAsync(response);
        
        if (updatedUser.UserPrincipal is not null)
        {
            principal = updatedUser.UserPrincipal;
        }
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));

        return response;
    }

    public async Task LogoutAsync()
    {
        var token = FetchCurrentUser().Token;
        var refreshToken = FetchCurrentUser().RefreshToken;

        if (token != string.Empty)
        {
            try
            {
                // Allow the continuation of logout even if token is no longer valid to revoke refresh token
                await _authService.Logout(token, refreshToken);
            }
            catch (HttpStatusCodeException) {}
        }
        
        if (_hubConnection is not null)
        {
            var userRole = FetchCurrentUser().UserPrincipal?.FindFirst(ClaimTypes.Role)?.Value;
            var userName = FetchCurrentUser().UserPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _hubConnection.SendAsync("RemoveFromGroup", userName, userRole);
        }
        
        await _patientAnalyticsUserService.RemoveUserFromStorageAsync();
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
    }

    private async Task RefreshAsync()
    {
        var principal = new ClaimsPrincipal();
        
        var storage = _patientAnalyticsUserService.GetAuthenticationDataMemoryStorage();
        
        var refreshResponse = await _authService.Refresh(storage.Token, new RefreshPayload
        {
            RefreshToken = storage.RefreshToken
        });
        
        var updatedUser = await _patientAnalyticsUserService.SaveUserInStorageAsync(refreshResponse);
        
        if (updatedUser.UserPrincipal is not null)
        {
            principal = updatedUser.UserPrincipal;
        }
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task ServiceWrapper(
        bool refreshing, 
        Action<string> serviceCallback,
        Action<bool> updateRefreshingState,
        Action<HttpStatusCodeException> updateExceptionState,
        Action handleLogout)
    {
        try
        {
            serviceCallback(FetchCurrentUser().Token);
        }
        catch (HttpStatusCodeException exception)
        {
            if (!refreshing && exception.StatusCode == 401)
            {
                try
                {
                    updateRefreshingState(true);

                    await RefreshAsync();
                    
                    // FetchCurrentUser().Token will be updated if RefreshAsync() is successful
                    serviceCallback(FetchCurrentUser().Token);
                }
                catch
                {
                    await LogoutAsync();
                    handleLogout();
                }
                finally
                {
                    updateRefreshingState(false);
                }
            }
            else
            {
                updateExceptionState(exception);
            }
        }  
    }
    
    public async Task ServiceWrapperAsync(
        bool refreshing, 
        Func<string, Task> serviceCallback,
        Action<bool> updateRefreshingState,
        Action<HttpStatusCodeException> updateExceptionState,
        Action handleLogout)
    {
        try
        {
            await serviceCallback(FetchCurrentUser().Token);
        }
        catch (HttpStatusCodeException exception)
        {
            if (!refreshing && exception.StatusCode == 401)
            {
                try
                {
                    updateRefreshingState(true);

                    await RefreshAsync();
                    
                    // FetchCurrentUser().Token will be updated if RefreshAsync() is successful
                    await serviceCallback(FetchCurrentUser().Token);
                }
                catch
                {
                    await LogoutAsync();
                    handleLogout();
                }
                finally
                {
                    updateRefreshingState(false);
                }
            }
            else
            {
                updateExceptionState(exception);
            }
        }  
    }
    
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = new ClaimsPrincipal();
        var memoryStorage = _patientAnalyticsUserService.GetAuthenticationDataMemoryStorage();

        if (memoryStorage.UserPrincipal is not null)
        {
            principal = memoryStorage.UserPrincipal;
        }
        
        return Task.FromResult(new AuthenticationState(principal));
    }

    private async void OnAuthenticationStateChangedAsync(Task<AuthenticationState> task)
    {
        var authenticationState = await task;
        
        _patientAnalyticsUserService.UpdateUserPrincipal(authenticationState.User);

        await EstablishHubConnection();
    }

    private async Task EstablishHubConnection()
    {
        if (_hubConnection is not null) await _hubConnection.DisposeAsync();
        
        var token = FetchCurrentUser().Token;

        if (token == string.Empty) return;
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_config["HubConnection:Url"]!, options =>
            {
                options.SkipNegotiation = true;
                options.Transports = HttpTransportType.WebSockets;
                options.AccessTokenProvider = () => Task.FromResult(token)!;
            })
            .AddJsonProtocol(protocolOptions =>
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                jsonOptions.Converters.Add(new JsonStringEnumConverter());

                protocolOptions.PayloadSerializerOptions = jsonOptions;
            })
            .Build();
        
        await _hubConnection.StartAsync();
        
        var userRole = FetchCurrentUser().UserPrincipal?.FindFirst(ClaimTypes.Role)?.Value;
        var userName = FetchCurrentUser().UserPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        await _hubConnection.SendAsync("AddToGroup", userName, userRole);
    }

    public HubConnection? GetHubConnection() => _hubConnection;
    
    public AuthenticationDataMemoryStorage FetchCurrentUser()
    {
        return _patientAnalyticsUserService.GetAuthenticationDataMemoryStorage();
    }

    public bool HasAdminPrivileges()
    {
        var userPrincipal = _patientAnalyticsUserService.GetAuthenticationDataMemoryStorage().UserPrincipal;

        return userPrincipal is not null && 
               (userPrincipal.HasClaim(ClaimTypes.Role, nameof(Role.SuperAdmin)) || 
                userPrincipal.HasClaim(ClaimTypes.Role, nameof(Role.Admin)));
    }
    
    public bool IsDoctor()
    {
        var userPrincipal = _patientAnalyticsUserService.GetAuthenticationDataMemoryStorage().UserPrincipal;

        return userPrincipal is not null && userPrincipal.HasClaim(ClaimTypes.Role, nameof(Role.Doctor));
    }
    
    
    public bool IsSuperAdmin()
    {
        var userPrincipal = _patientAnalyticsUserService.GetAuthenticationDataMemoryStorage().UserPrincipal;

        return userPrincipal is not null && userPrincipal.HasClaim(ClaimTypes.Role, nameof(Role.SuperAdmin));
    }

    public void Dispose()
    {
        AuthenticationStateChanged -= OnAuthenticationStateChangedAsync;
    }
}