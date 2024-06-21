using System.ComponentModel.DataAnnotations.Schema;

namespace PatientAnalytics.Models.PatientMetrics;

public class PatientBloodPressure
{
    public int Id { get; protected set; }
    public int PatientId { get; protected set; }
    public int DoctorId { get; protected set; }
    
    [ForeignKey("DoctorId")]
    public User? Doctor { get; protected set; }
    
    [ForeignKey("PatientId")]
    public Patient? Patient { get; protected set; }
    
    public DateTime DateCreated { get; protected set; }
    public double BloodPressureSystolic { get; protected set; }
    public double BloodPressureDiastolic { get; protected set; }
    
    [NotMapped]
    public string Status { get; protected set; }

    public static PatientBloodPressure CreateFromPayload(
        int patientId, 
        int doctorId,
        PatientBloodPressurePayload payload)
    {
        return new PatientBloodPressure
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DateCreated = DateTime.Now,
            BloodPressureSystolic = payload.BloodPressureSystolic,
            BloodPressureDiastolic = payload.BloodPressureDiastolic
        };
    }

    public PatientBloodPressure Formatted()
    {
        if (BloodPressureSystolic < 120 && BloodPressureDiastolic < 80)
        {
            Status = "Normal";
        }
        else if (BloodPressureSystolic is >= 120 and <= 129 && BloodPressureDiastolic < 80)
        {
            Status = "Elevated";
        }
        else if (BloodPressureSystolic is >= 130 and <= 139 || BloodPressureDiastolic is >= 80 and <= 89)
        {
            Status = "High Blood Pressure - Stage 1";
        }
        else if (BloodPressureSystolic is >= 140 and <= 180 || BloodPressureDiastolic is >= 90 and <= 120)
        {
            Status = "High Blood Pressure - Stage 2";
        }
        else
        {
            Status = "Hypertensive Crisis";
        }

        return this;
    }
}

public class PatientBloodPressurePayload
{
    public double BloodPressureSystolic { get; set; }
    public double BloodPressureDiastolic { get; set; }

    public void SetBloodPressure(double bloodPressureSystolic, double bloodPressureDiastolic)
    {
        BloodPressureSystolic = bloodPressureSystolic;
        BloodPressureDiastolic = bloodPressureDiastolic;
    }
}