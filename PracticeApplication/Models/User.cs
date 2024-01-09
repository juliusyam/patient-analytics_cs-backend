
using PracticeApplication.Models;

public class User : Person
{
    private User(
        DateTime dateOfBirth, string gender, string email, string? address, DateTime dateCreated, DateTime? dateEdited
        ) : base(dateOfBirth, gender, email, address, dateCreated, dateEdited)
    {
        
    }

    public static User CreateUser(CreatePayload payload)
    {
        DateTime dateCreated = DateTime.Now;

        return new User(
            payload.DateOfBirth,
            payload.Gender,
            payload.Email,
            payload.Address,
            dateCreated,
            null
        );
    }
}