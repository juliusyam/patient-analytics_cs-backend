using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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
            DateOfBirth = payload.DateOfBirth.ToUniversalTime(),
            Gender = payload.Gender,
            Email = payload.Email,
            PasswordHash = passwordHash,
            Username = payload.Username,
            Role = role,
            Address = payload.Address,
            FirstName = payload.FirstName,
            LastName = payload.LastName,
            DateCreated = DateTime.UtcNow
        };
    }

    public void UpdateAccountInfo(UserAccountInfoPayload payload)
    {
        DateOfBirth = payload.DateOfBirth.ToUniversalTime();
        Gender = payload.Gender;
        Address = payload.Address;
        FirstName = payload.FirstName;
        LastName = payload.LastName;
        DateEdited = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsDeactivated = true;
    }

    public void Activate()
    {
        IsDeactivated = false;
    }

    [MaxLength(30)] public string Username { get; set; } = null!;

    [JsonIgnore, MaxLength(255)] public string PasswordHash { get; set; } = null!;

    [MaxLength(30),
     RegularExpression("^SuperAdmin$|^Admin$|^Doctor$",
         ErrorMessage = "Invalid Role Value. Role Value can either be SuperAdmin, Admin or Doctor")]
    public string Role { get; set; } = null!;

    public bool IsDeactivated { get; set; }
    
    [JsonIgnore] public ICollection<UserRefresh> UserRefreshes { get; } = new List<UserRefresh>();
}

public class UserAccountInfoPayload
{
    [Required(ErrorMessage = "Date of Birth is required.")]
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Gender is required.")]
    public string Gender { get; set; } = null!;

    [Required(ErrorMessage = "First Name is required.")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last Name is required.")]
    public string? LastName { get; set; }

    public string? Address { get; set; }

    public static UserAccountInfoPayload CreatePayloadFromUser(User user)
    {
        return new UserAccountInfoPayload
        {
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address
        };
    }
}