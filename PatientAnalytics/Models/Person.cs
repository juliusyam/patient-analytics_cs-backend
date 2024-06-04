using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;

namespace PatientAnalytics.Models;

public class Person
{
    public Person(DateTime dateOfBirth, string gender, string email, string? address, string? firstName, string? lastName, DateTime dateCreated,
        DateTime? dateEdited)
    {
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Email = email;
        Address = address;
        DateCreated = dateCreated;
        DateEdited = dateEdited;
        FirstName = firstName;
        LastName = lastName;
    }

    [Key]
    public int Id { get; protected set; }
    public DateTime DateOfBirth { get; protected set; }
    public string Gender { get; protected set; }
    public string? FirstName { get; protected set; }
    public string? LastName { get; protected set; }
    
    [EmailAddress(ErrorMessage = "Email address format is invalid.")]
    public string Email { get; protected set; }
    public string? Address { get; protected set; }
    public DateTime DateCreated { get; protected set; }
    public DateTime? DateEdited { get; protected set; }
}