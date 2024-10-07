using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public AuthService(
        [FromServices] Context context, 
        JwtService jwtService, 
        IConfiguration config, 
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _config = config;
        _context = context;
        _jwtService = jwtService;
        _localized = localized;
    }

    public async Task<LoginResponse> Login(LoginPayload loginPayload)
    {
        var user = Authenticate(loginPayload);

        var (refreshToken, refreshEntry) = UserRefresh.CreateRefreshForUser(user.Id, _config);
        
        _context.UserRefreshes.Add(refreshEntry);

        await _context.SaveChangesAsync();
        
        var token = _jwtService.GenerateJwt(user);
        
        return new LoginResponse(token, refreshToken, user);
    }

    public async Task<LoginResponse> Refresh(string token, RefreshPayload payload)
    {
        var user = _jwtService.GetUserWithExpiredJwt(token, payload.RefreshToken);

        var newToken = _jwtService.GenerateJwt(user);

        var refreshTokenHash = Password.HashPassword(payload.RefreshToken, _config);
        
        var existingRefreshEntry = user.UserRefreshes
            .FirstOrDefault(ur => ur.RefreshTokenHash == refreshTokenHash);

        if (existingRefreshEntry is not null) _context.UserRefreshes.Remove(existingRefreshEntry);

        var (newRefreshToken, newRefreshEntry) = UserRefresh.CreateRefreshForUser(user.Id, _config);
        
        _context.UserRefreshes.Add(newRefreshEntry);
        
        await _context.SaveChangesAsync();

        return new LoginResponse(newToken, newRefreshToken, user);
    }

    public async Task<IActionResult> Logout(string token, string? refreshToken)
    {
        var user = _jwtService.GetUserWithJwt(token);

        if (refreshToken is not null)
        {
            var refreshTokenHash = Password.HashPassword(refreshToken, _config);

            var existingRefreshEntry = user.UserRefreshes
                .FirstOrDefault(ur => ur.RefreshTokenHash == refreshTokenHash);

            if (existingRefreshEntry is not null) _context.UserRefreshes.Remove(existingRefreshEntry);
        }
        
        _context.Users.Update(user);

        await _context.SaveChangesAsync();

        return new NoContentResult();
    }

    private User Authenticate(LoginPayload loginPayload)
    {
        var currentUser = _context.Users.FirstOrDefault(user => user.Username == loginPayload.Username);

        if (currentUser == null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, 
                _localized["AuthError_UserNotFound"]); 
        }

        var passwordHash = Password.HashPassword(loginPayload.Password, _config);

        if (currentUser.PasswordHash != passwordHash) 
        { 
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, 
                _localized["AuthError_WrongPassword"]); 
        }

        if (currentUser.IsDeactivated)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden,
                _localized["AuthError_UserIsDeactivated"]);
        }
        
        return currentUser;
    }
}