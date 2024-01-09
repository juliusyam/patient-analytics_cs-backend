namespace PracticeApplication.Models;

public class Person
{
    protected Person(DateTime dateOfBirth, string gender, string email, string? address, DateTime dateCreated,
        DateTime? dateEdited)
    {
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Email = email;
        Address = address;
        DateCreated = dateCreated;
        DateEdited = dateEdited;
    }
    
    public class CreatePayload
    {
        public CreatePayload(DateTime dateOfBirth, string gender, string email, string? address = null)
        {
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Email = email;
            Address = address;
        }
        
        public DateTime DateOfBirth { get; private set; }
        public string Gender { get; private set; }
        public string Email { get; private set; }
        public string? Address { get; private set; }
    }
    
    public int Id { get; protected set; }
    public DateTime DateOfBirth { get; protected set; }
    public string Gender { get; protected set; }
    public string Email { get; protected set; }
    public string? Address { get; protected set; }
    public DateTime DateCreated { get; protected set; }
    public DateTime? DateEdited { get; protected set; }
}