using System.ComponentModel.DataAnnotations;

namespace PatientAnalytics.Models.Auth;

public class RegistrationPayload
{
    [Required(ErrorMessage = "Date of Birth is required.")]
    public DateTime DateOfBirth { get; set; } = DateTime.Now;
    
    [Required(ErrorMessage = "Gender is required.")]
    public string Gender { get; set; }
    
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "First Name is required.")]
    public string? FirstName { get; set; }
    
    [Required(ErrorMessage = "Last Name is required.")]
    public string? LastName { get; set; }
    
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; }
    
    public string? Address { get; set; }
}