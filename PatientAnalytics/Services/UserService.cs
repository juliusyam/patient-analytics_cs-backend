using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Utils;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Services;

public class UserService
{
    private readonly IConfiguration _config;
    private readonly Context _context;
    private readonly JwtService _jwtService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public UserService(
        [FromServices] Context context, 
        IConfiguration config, 
        JwtService jwtService,
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _context = context;
        _config = config;
        _jwtService = jwtService;
        _localized = localized;
    }

    public List<User> GetDoctors(string token)
    {
        ValidateIsAdmin(token, out _);

        return _context.Users.Where(u => u.Role == "Doctor").ToList();
    }

    public List<User> GetAdmins(string token)
    {
        ValidateIsAdmin(token, out _);

        return _context.Users.Where(u => u.Role == "Admin").ToList();
    }

    public List<User> GetSuperAdmins(string token)
    {
        ValidateIsSuperAdmin(token, out _);
        
        return _context.Users.Where(u => u.Role == "SuperAdmin").ToList();
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