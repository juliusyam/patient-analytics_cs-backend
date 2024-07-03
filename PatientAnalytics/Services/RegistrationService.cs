using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Hubs;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Utils;
using PatientAnalytics.Utils.Localization;
using System.Security.Claims;

namespace PatientAnalytics.Services;

public class RegistrationService
{
    private readonly IConfiguration _config;
    private readonly Context _context;
    private readonly JwtService _jwtService;
    private readonly IHubContext<PatientHub> _hubContext;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public RegistrationService(
            Context context, 
            IConfiguration config, 
            JwtService jwtService,
            IHubContext<PatientHub> hubContext,
            IStringLocalizer<ApiResponseLocalized> localized)
    {
        _context = context;
        _config = config;
        _jwtService = jwtService;
        _hubContext = hubContext;
        _localized = localized;
    }
    
    public async Task<RegisterResponse> RegisterUser(string authorization, RegistrationPayload payload, string role)
    {
        switch (role)
        {
            case "SuperAdmin":
                ValidateIsSuperAdmin(authorization, out _);
                break;
            case "Admin":
            case "Doctor":
                ValidateIsAdmin(authorization, out _);
                break;
            default:
                throw new HttpStatusCodeException(StatusCodes.Status422UnprocessableEntity,
                    "Invalid Role Value. Role Value can either be SuperAdmin, Admin or Doctor");
        }
        
        var passwordLength = int.Parse(_config["Auth:PasswordLength"] ?? "10");
        var password = payload.Password;
        var validPassword = Password.IsPasswordValid(password, _config);
        var isHashLeaked = await Password.IsPasswordLeaked(password);

        if (isHashLeaked)
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                _localized["AuthError_WeakPassword_IsHashLeaked"]
            );

        if (!validPassword)
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                string.Format(_localized["AuthError_WeakPassword_Validation"], passwordLength)
            );

        var passwordHash = Password.HashPassword(password, _config);
        
        UsernameIsNotOccupied(payload.Username);
        
        EmailIsNotOccupied(payload.Email);

        var user = User.CreateUser(passwordHash, payload, role);
        
        _context.Users.Add(user);

        await _context.SaveChangesAsync();
        
        var token = _jwtService.GenerateJwt(user);

        var userNameIdentifier = _jwtService.DecodeJwt(token)?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userNameIdentifier is not null)
        {
            switch (role)
            {
                case "SuperAdmin":
                    await _hubContext.Clients.Group("SuperAdmin").SendAsync("ReceiveNewSuperAdmin", user);
                    break;
                case "Admin":
                    await _hubContext.Clients.Group("Admin").SendAsync("ReceiveNewAdmin", user);
                    break;
                case "Doctor":
                    await _hubContext.Clients.Group("Admin").SendAsync("ReceiveNewDoctor", user);
                    break;
            }
        }
    
        return new RegisterResponse(user, token);
    }

    private void EmailIsNotOccupied(string email)
    {
        var userWithIdenticalEmail = _context.Users.FirstOrDefault(u => u.Email == email);
        
        if (userWithIdenticalEmail is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                string.Format(_localized["RepeatedError_Email"], email));
        }
    }

    private void UsernameIsNotOccupied(string username)
    {
        var userWithIdenticalUsername = _context.Users.FirstOrDefault(u => u.Username == username);
        
        if (userWithIdenticalUsername is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                string.Format(_localized["RepeatedError_Username"], username));
        }
    }
    
    private void ValidateIsAdmin(string token, out User verifiedUser)
    {
        var user = _jwtService.GetUserWithJwt(token);

        if (user.Role != "SuperAdmin" && user.Role != "Admin")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, 
                _localized["AuthError_Unauthorized"]);
        }

        verifiedUser = user;
    }
    
    private void ValidateIsSuperAdmin(string token, out User verifiedUser)
    {
        var user = _jwtService.GetUserWithJwt(token);

        if (user.Role != "SuperAdmin")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, 
                _localized["AuthError_Unauthorized"]);
        }

        verifiedUser = user;
    }
}