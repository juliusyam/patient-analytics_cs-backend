namespace PatientAnalytics.Models;

public class Patient : Person
{
    private Patient(
        int doctorId, DateTime dateOfBirth, string gender, string email, string? address, string? firstName, string? lastName, DateTime dateCreated, DateTime? dateEdited
        ) : base(dateOfBirth, gender, email, address, firstName, lastName, dateCreated, dateEdited)
    {
        DoctorId = doctorId;
    }

    public static Patient CreatePatient(int doctorId, Person payload)
    {
        DateTime dateCreated = DateTime.Now;

        return new Patient(
            doctorId,
            payload.DateOfBirth,
            payload.Gender,
            payload.Email,
            payload.Address,
            payload.FirstName,
            payload.LastName,
            dateCreated,
            null
        );
    }

    public void UpdatePatient(Person payload)
    {
        DateOfBirth = payload.DateOfBirth;
        Gender = payload.Gender;
        Email = payload.Email;
        Address = payload.Address;
        FirstName = payload.FirstName;
        LastName = payload.LastName;
        DateEdited = DateTime.Now;
    }
    
    public int DoctorId { get; private set; }
}