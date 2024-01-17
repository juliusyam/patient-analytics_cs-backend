namespace PatientAnalytics.Models.Auth;

public class RegistrationPayload
{
    public RegistrationPayload(DateTime dateOfBirth, string gender, string email, string username, string password, string? address = null, string? firstName = null, string? lastName = null)
    {
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Email = email;
        Username = username;
        Password = password;
        Address = address;
        FirstName = firstName;
        LastName = lastName;
    }
        
    public DateTime DateOfBirth { get; private set; }
    public string Gender { get; private set; }
    public string Username { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string Password { get; set; }
    public string Email { get; private set; }
    public string? Address { get; private set; }
}