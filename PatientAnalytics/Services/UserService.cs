using Microsoft.AspNetCore.Mvc;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Utils;

namespace PatientAnalytics.Services;

public class UserService
{
    private readonly IConfiguration _config;
    private readonly Context _context;

    public UserService([FromServices] Context context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<(User, string)> CreateInitialSuperAdmin()
    {
        var payload = new RegistrationPayload(
            DateTime.Parse("1990-02-17T06:10:35.950Z"),
            "Male",
            "superadmin@patient-analytics.co.uk",
            "superadmin",
            Password.GeneratePassword()
        );
        
        var passwordHash = Password.HashPassword(payload.Password, _config);
        
        var user = User.CreateUser(passwordHash, payload, "SuperAdmin");
        
        _context.Users.Add(user);

        await _context.SaveChangesAsync();
        
        return (user, payload.Password);
    }

    public async Task<User> CreateUser(string passwordHash, RegistrationPayload payload, string role)
    {
        var userWithIdenticalEmail = _context.Users.FirstOrDefault(u => u.Email == payload.Email);
        var userWithIdenticalUsername = _context.Users.FirstOrDefault(u => u.Username == payload.Username);
        
        if (userWithIdenticalUsername is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                $"Username {payload.Username} already taken");
        }

        if (userWithIdenticalEmail is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                $"Email address {payload.Email} already taken");
        }

        var user = User.CreateUser(passwordHash, payload, role);
        _context.Users.Add(user);

        await _context.SaveChangesAsync();
        
        return user;
    }
}