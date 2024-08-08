using System.ComponentModel.DataAnnotations;

namespace PatientAnalytics.Models.Auth;

public class RegistrationPayload
{
    [Required(ErrorMessage = "Date of Birth is required.")]
    public DateTime DateOfBirth { get; set; } = DateTime.Now;
    
    [Required(ErrorMessage = "Gender is required.")]
    public string Gender { get; set; } = null!;
    
    [Required(ErrorMessage = "Username is required."), RegularExpression("^[A-Za-z][A-Za-z0-9_]{7,29}$", ErrorMessage = "Username format is invalid. Username must start with an alphabet, be between 8-30 characters, and without space.")]
    public string Username { get; set; } = null!;
    
    [Required(ErrorMessage = "First Name is required.")]
    public string? FirstName { get; set; }
    
    [Required(ErrorMessage = "Last Name is required.")]
    public string? LastName { get; set; }
    
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = null!;
    
    [Required(ErrorMessage = "Email is required."), RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Email format is invalid")]
    public string Email { get; set; } = null!;
    
    public string? Address { get; set; }
}