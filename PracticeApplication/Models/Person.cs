namespace PracticeApplication.Models;

public class Person
{
    protected Person(DateTime dateOfBirth, string gender, string email, string? address, string? firstName, string? lastName, DateTime dateCreated,
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
    
    public class CreatePayload
    {
        public CreatePayload(DateTime dateOfBirth, string gender, string email, string username, string password,  string? address = null, string? firstName = null, string? lastName = null)
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
    
    public int Id { get; protected set; }
    public DateTime DateOfBirth { get; protected set; }
    public string Gender { get; protected set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string Email { get; protected set; }
    public string? Address { get; protected set; }
    public DateTime DateCreated { get; protected set; }
    public DateTime? DateEdited { get; protected set; }
}