using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PatientAnalytics.Middleware;

namespace PatientAnalytics.Models.PatientMetrics;

public class PatientHeight
{
    public int Id { get; protected set; }
    public int PatientId { get; protected set; }
    public int DoctorId { get; protected set; }
    
    [ForeignKey("DoctorId")]
    public User? Doctor { get; protected set; }
    
    [ForeignKey("PatientId")]
    public Patient? Patient { get; protected set; }
    public DateTime DateCreated { get; protected set; }
    public double HeightCm { get; protected set; }
    
    [NotMapped]
    public string? HeightInFormatted { get; protected set; }
    
    [NotMapped]
    public string? HeightCmFormatted { get; protected set; }
    
    [NotMapped]
    public string? HeightFtFormatted { get; protected set; }

    public static PatientHeight CreateFromPayload(int patientId, int doctorId, PatientHeightPayload payload)
    {
        double heightCm;

        switch (payload.Unit)
        {
            case "Cm":
                heightCm = payload.Height;
                break;
            case "In":
                heightCm = payload.Height * 2.54;
                break;
            default:
                throw new HttpStatusCodeException(StatusCodes.Status422UnprocessableEntity,
                    "Invalid Unit Value. Unit must be either Cm or In.");
        }

        return new PatientHeight
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DateCreated = DateTime.UtcNow,
            HeightCm = heightCm
        };
    }

    public PatientHeight Formatted()
    {
        var heightIn = HeightCm / 2.54;
        
        HeightCmFormatted = HeightCm.ToString("0.##");
        
        HeightInFormatted = heightIn.ToString("0.##");
        
        HeightFtFormatted = $"{(heightIn - heightIn % 12) / 12}'{(int)heightIn % 12}";
        
        return this;
    }
}

public class PatientHeightPayload
{
    public double Height { get; set; }

    [RegularExpression("^Cm$|^In$", ErrorMessage = "Invalid Unit Value. Unit must be either Cm or In.")]
    public string Unit { get; set; } = null!;
}