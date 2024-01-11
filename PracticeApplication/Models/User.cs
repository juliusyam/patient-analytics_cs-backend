
using PracticeApplication.Models;

public class User : Person
{
    private User(
        DateTime dateOfBirth, string gender, string email, string passwordHash, string username, string? address, string? firstName, string? lastName, DateTime dateCreated, DateTime? dateEdited
        ) : base(dateOfBirth, gender, email, address, firstName,lastName, dateCreated, dateEdited)
    {
        PasswordHash = passwordHash;
        Username = username;
    }

    public static User CreateUser(CreatePayload payload)
    {
        DateTime dateCreated = DateTime.Now;

        return new User(
            payload.DateOfBirth,
            payload.Gender,
            payload.Email,
            payload.Password,
            payload.Username,
            payload.Address,
            payload.FirstName,
            payload.LastName,
            dateCreated,
            null
        );
    }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
}