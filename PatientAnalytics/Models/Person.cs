using System.ComponentModel.DataAnnotations;

namespace PatientAnalytics.Models;

public class Person
{
    [Key]
    public int Id { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    [EmailAddress(ErrorMessage = "Email address format is invalid.")]
    public string Email { get; set; } = null!;
    public string? Address { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateEdited { get; set; }
}

public class PersonPayload : UserAccountInfoPayload
{
    [Required(ErrorMessage = "Email is required."), RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Email format is invalid")]
    public string Email { get; set; } = null!;
}
