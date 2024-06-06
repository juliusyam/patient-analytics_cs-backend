using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Blazor.Localization;
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

    public async Task<(User, string)> CreateInitialSuperAdmin()
    {
        var payload = new RegistrationPayload
        {
            DateOfBirth = DateTime.Parse("1990-02-17T06:10:35.950Z"),
            Gender = "Male",
            Email = "superadmin@patient-analytics.co.uk",
            Username = "superadmin",
            Password = Password.GeneratePassword()
        };
        
        var passwordHash = Password.HashPassword(payload.Password, _config);
        
        var user = User.CreateUser(passwordHash, payload, "SuperAdmin");
        
        _context.Users.Add(user);

        await _context.SaveChangesAsync();
        
        return (user, payload.Password);
    }

    public async Task<User> CreateUser(string authorization, string passwordHash, RegistrationPayload payload, string role)
    {
        if (role == "SuperAdmin")
        {
            ValidateIsSuperAdmin(authorization, out _);
        }
        else
        {
            ValidateIsAdmin(authorization, out _);
        }
        
        var userWithIdenticalEmail = _context.Users.FirstOrDefault(u => u.Email == payload.Email);
        var userWithIdenticalUsername = _context.Users.FirstOrDefault(u => u.Username == payload.Username);
        
        if (userWithIdenticalUsername is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                string.Format(_localized["RepeatedError_Username"], payload.Username));
        }

        if (userWithIdenticalEmail is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                string.Format(_localized["RepeatedError_Email"], payload.Email));
        }

        var user = User.CreateUser(passwordHash, payload, role);
        _context.Users.Add(user);

        await _context.SaveChangesAsync();
        
        return user;
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