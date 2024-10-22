using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models.PatientMetrics.Units;

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
        Enum.TryParse(payload.Unit, out HeightUnit unit);

        var heightCm = unit switch
        {
            HeightUnit.Cm => payload.Height,
            HeightUnit.In => payload.Height * 2.54,
            _ => throw new HttpStatusCodeException(StatusCodes.Status422UnprocessableEntity,
                "Invalid Unit Value. Unit must be either Cm or In.")
        };

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

public class PatientHeightPayload : IValidatableObject
{
    public double Height { get; init; }

    public string Unit { get; init; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var unitIsValid = Enum.TryParse(Unit, out HeightUnit unit) && unit != HeightUnit.Ft;

        if (Height <= 0.0)
        {
            yield return new ValidationResult(
                "Height value must be higher than 0.",
                new[] { nameof(Height) });
        }

        if (!unitIsValid)
        {
            yield return new ValidationResult(
                "Invalid Unit Value. Unit must be either Cm or In.",
                new[] { nameof(Unit) });
        }
    }

    public static bool TryParse(PatientHeightFormValues formValues, out PatientHeightPayload payload)
    {
        if (formValues.Height > 0.0 && formValues.Unit != HeightUnit.Ft)
        {
            payload = new PatientHeightPayload
            {
                Height = formValues.Height.Value,
                Unit = formValues.Unit.ToString()
            };
            return true;
        }

        payload = new();
        return false;
    }
}

public class PatientHeightFormValues
{
    public double? Height { get; set; }

    public HeightUnit Unit { get; set; } = HeightUnit.Cm;

    public void Reset()
    {
        Height = null;
        Unit = HeightUnit.Cm;
    }
}