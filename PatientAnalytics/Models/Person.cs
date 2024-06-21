using System.ComponentModel.DataAnnotations;

namespace PatientAnalytics.Models;

public class Person
{
    [Key]
    public int Id { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    [EmailAddress(ErrorMessage = "Email address format is invalid.")]
    public string Email { get; set; }
    public string? Address { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateEdited { get; set; }
}

public class PersonPayload
{
    [Required(ErrorMessage = "Date of Birth is required.")]
    public DateTime DateOfBirth { get; set; } = DateTime.Now;
    
    [Required(ErrorMessage = "Gender is required.")]
    public string Gender { get; set; }
    
    [Required(ErrorMessage = "First Name is required.")]
    public string? FirstName { get; set; }
    
    [Required(ErrorMessage = "Last Name is required.")]
    public string? LastName { get; set; }
    
    [Required(ErrorMessage = "Email is required."), RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Email format is invalid")]
    public string Email { get; set; }
    
    public string? Address { get; set; }
}
