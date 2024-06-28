using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models.Auth;

namespace PatientAnalytics.Models;

public class User : Person
{
    public static User CreateUser(string passwordHash, RegistrationPayload payload, string role)
    {
        var userRoles = new[] { "SuperAdmin", "Admin", "Doctor" };
        if (!userRoles.Contains(role)) 
            throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, 
                "Invalid Role Value. Role Value can either be SuperAdmin, Admin or Doctor");

        return new User
        {
            DateOfBirth = payload.DateOfBirth,
            Gender = payload.Gender,
            Email = payload.Email,
            PasswordHash = passwordHash,
            Username = payload.Username,
            Role = role,
            Address = payload.Address,
            FirstName = payload.FirstName,
            LastName = payload.LastName,
            DateCreated = DateTime.Now
        };
    }

    public ClaimsPrincipal ToClaimsPrincipal()
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimTypes.NameIdentifier, Username),
                new Claim(ClaimTypes.Sid, Id.ToString()),
                new Claim(ClaimTypes.Role, Role),
            },
            "Custom Authentication"
            ));
    }

    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    
    [RegularExpression("^SuperAdmin$|^Admin$|^Doctor$", ErrorMessage = "Invalid Role Value. Role Value can either be SuperAdmin, Admin or Doctor")]
    public string Role { get; private set; }
    
    public ICollection<UserRefresh> UserRefreshes { get; } = new List<UserRefresh>();
}