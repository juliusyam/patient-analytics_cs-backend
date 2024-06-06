using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Utils;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Services;

public class AuthService
{
    private readonly IConfiguration _config;
    private readonly Context _context;
    private readonly JwtService _jwtService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public AuthService([FromServices] Context context, JwtService jwtService, IConfiguration config, IStringLocalizer<ApiResponseLocalized> localized)
    {
        _config = config;
        _context = context;
        _jwtService = jwtService;
        _localized = localized;
    }

    public LoginResponse Login(LoginPayload loginPayload)
    {
        var user = Authenticate(loginPayload);
        var token = _jwtService.GenerateJwt(user);
        return new LoginResponse(token, user);
    }

    private User Authenticate(LoginPayload loginPayload)
    {
        User? currentUser = _context.Users.FirstOrDefault(user => user.Username == loginPayload.Username);

        if (currentUser == null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, _localized["AuthError_UserNotFound"]); 
        }

        var passwordHash = Password.HashPassword(loginPayload.Password, _config);

        if (currentUser.PasswordHash != passwordHash) 
        { 
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["AuthError_WrongPassword"]); 
        } 
        return currentUser;
    }
}