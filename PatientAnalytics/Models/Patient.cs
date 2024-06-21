using PatientAnalytics.Models.PatientMetrics;

namespace PatientAnalytics.Models;

public class Patient : Person
{
    public static Patient CreatePatient(int doctorId, PersonPayload payload)
    {
        var dateCreated = DateTime.Now;

        return new Patient
        {
            DoctorId = doctorId,
            DateOfBirth = payload.DateOfBirth,
            Gender = payload.Gender,
            Email = payload.Email,
            Address = payload.Address,
            FirstName = payload.FirstName,
            LastName = payload.LastName,
            DateCreated = dateCreated,
            DateEdited = null
        };
    }

    public void UpdatePatient(PersonPayload payload)
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

    public ICollection<PatientBloodPressure> BloodPressures { get; } = new List<PatientBloodPressure>();
    public ICollection<PatientTemperature> Temperatures { get; } = new List<PatientTemperature>();
    public ICollection<PatientHeight> Heights { get; } = new List<PatientHeight>();
    public ICollection<PatientWeight> Weights { get; } = new List<PatientWeight>();
}
