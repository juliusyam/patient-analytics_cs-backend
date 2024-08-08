using System.ComponentModel.DataAnnotations;
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

    [MaxLength(30)]
    public string Username { get; set; } = null!;
    
    [MaxLength(255)]
    public string PasswordHash { get; set; } = null!;
    
    [MaxLength(30), RegularExpression("^SuperAdmin$|^Admin$|^Doctor$", ErrorMessage = "Invalid Role Value. Role Value can either be SuperAdmin, Admin or Doctor")]
    public string Role { get; set; } = null!;
    
    public ICollection<UserRefresh> UserRefreshes { get; } = new List<UserRefresh>();
}