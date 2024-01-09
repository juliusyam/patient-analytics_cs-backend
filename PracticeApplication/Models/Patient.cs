namespace PracticeApplication.Models;

public class Patient : Person
{
    private Patient(
        int doctorId, DateTime dateOfBirth, string gender, string email, string? address, DateTime dateCreated, DateTime? dateEdited
        ) : base(dateOfBirth, gender, email, address, dateCreated, dateEdited)
    {
        DoctorId = doctorId;
    }

    public static Patient CreatePatient(int doctorId, CreatePayload payload)
    {
        DateTime dateCreated = DateTime.Now;

        return new Patient(
            doctorId,
            payload.DateOfBirth,
            payload.Gender,
            payload.Email,
            payload.Address,
            dateCreated,
            null
        );
    }
    
    public int DoctorId { get; private set; }
}