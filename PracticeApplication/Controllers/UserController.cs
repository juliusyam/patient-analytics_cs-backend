using Microsoft.AspNetCore.Mvc;
using PracticeApplication.Middleware;
using PracticeApplication.Models;

namespace PracticeApplication.Controllers;

[ApiController]
[Route("/users")]
public class UserController
{
    [HttpGet("{userId}", Name = "GetUser")]
    public User? GetUserById([FromServices] Context context, [FromRoute] int userId)
    {
        var user = context.Users.FirstOrDefault(u => u.Id == userId);

        if (user is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, $"Unable to find user with id: {userId}");
        }
        
        return user;
    }
    
    [HttpPost(Name = "CreateUser")]
    public async Task<User> CreateUser([FromServices] Context context, [FromBody] Person.CreatePayload payload)
    {
        var userWithIdenticalEmail = context.Users.FirstOrDefault(u => u.Email == payload.Email);

        if (userWithIdenticalEmail is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, $"Email address {payload.Email} already taken");
        }
        
        var user = User.CreateUser(payload);
        context.Users.Add(user);

        await context.SaveChangesAsync();
        
        return user;
    }
}