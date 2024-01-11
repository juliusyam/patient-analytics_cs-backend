using Microsoft.AspNetCore.Mvc;
using PracticeApplication.Middleware;
using PracticeApplication.Models;
using PracticeApplication.Models.Auth;

namespace PracticeApplication.Services;

public class UserService
{
    private readonly Context _context;

    public UserService([FromServices] Context context)
    {
        _context = context;
    }
    
    public async Task<User> CreateUser(string passwordHash, RegistrationPayload payload)
    {
        var userWithIdenticalEmail = _context.Users.FirstOrDefault(u => u.Email == payload.Email);
        var userWithIdenticalUsername = _context.Users.FirstOrDefault(u => u.Username == payload.Username);
        
        if (userWithIdenticalUsername is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, $"Username {payload.Username} already taken");
        }

        if (userWithIdenticalEmail is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, $"Email address {payload.Email} already taken");
        }
        
        var user = User.CreateUser(passwordHash, payload);
        _context.Users.Add(user);

        await _context.SaveChangesAsync();
        
        return user;
    }
}