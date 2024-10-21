using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PatientAnalytics.Middleware;

namespace PatientAnalytics.Models.PatientMetrics;

public class PatientWeight
{
    public int Id { get; protected set; }
    public int PatientId { get; protected set; }
    public int DoctorId { get; protected set; }
    
    [ForeignKey("DoctorId")]
    public User? Doctor { get; protected set; }
    
    [ForeignKey("PatientId")]
    public Patient? Patient { get; protected set; }
    
    public DateTime DateCreated { get; protected set; }
    public double WeightKg { get; protected set; }
    
    [NotMapped]
    public string? WeightKgFormatted { get; protected set; }
    
    [NotMapped]
    public string? WeightStFormatted { get; protected set; }
    
    [NotMapped]
    public string? WeightLbFormatted { get; protected set; }

    public static PatientWeight CreateFromPayload(int patientId, int doctorId, PatientWeightPayload payload)
    {
        double weightKg;

        switch (payload.Unit)
        {
            case "Kg":
                weightKg = payload.Weight;
                break;
            case "Lb":
                weightKg = payload.Weight * 0.45359237;
                break;
            case "St":
                weightKg = payload.Weight * 6.35029497;
                break;
            default:
                throw new HttpStatusCodeException(StatusCodes.Status422UnprocessableEntity,
                    "Invalid Unit Value. Unit must be either Kg, Lb or St.");
        }

        return new PatientWeight
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DateCreated = DateTime.UtcNow,
            WeightKg = weightKg
        };
    }

    public PatientWeight Formatted()
    {
        WeightKgFormatted = WeightKg.ToString("0.##");
        
        WeightLbFormatted = (WeightKg * 2.2046226218).ToString("0.##");
        
        WeightStFormatted = (WeightKg * 0.157473).ToString("0.##");
        
        return this;
    }
}

public class PatientWeightPayload
{
    public double Weight { get; set; }

    [RegularExpression("^Kg$|^Lb$|^St$", ErrorMessage = "Invalid Unit Value. Unit must be either Kg, Lb or St.")]
    public string Unit { get; set; } = null!;
}