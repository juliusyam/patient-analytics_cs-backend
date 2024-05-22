using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models.Auth;

namespace PatientAnalytics.Models;

public class User : Person
{
    private User(
        DateTime dateOfBirth, string gender, string email, string passwordHash, string username, string role, string? address, string? firstName, string? lastName, DateTime dateCreated, DateTime? dateEdited
    ) : base(dateOfBirth, gender, email, address, firstName, lastName, dateCreated, dateEdited)
    {
        PasswordHash = passwordHash;
        Username = username;
        Role = role;
    }

    public static User CreateUser(string passwordHash, RegistrationPayload payload, string role)
    {
        var userRoles = new[] { "SuperAdmin", "Admin", "Doctor" };
        if (!userRoles.Contains(role)) throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, "Invalid Role Value. Role Value can either be SuperAdmin, Admin or Doctor");
        
        var dateCreated = DateTime.Now;

        return new User(
            payload.DateOfBirth,
            payload.Gender,
            payload.Email,
            passwordHash,
            payload.Username,
            role,
            payload.Address,
            payload.FirstName,
            payload.LastName,
            dateCreated,
            null
        );
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
}