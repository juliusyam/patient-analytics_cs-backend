using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models.PatientMetrics.Units;

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
        Enum.TryParse(payload.Unit, out WeightUnit unit);

        var weightKg = unit switch
        {
            WeightUnit.Kg => payload.Weight,
            WeightUnit.Lb => payload.Weight * 0.45359237,
            WeightUnit.St => payload.Weight * 6.35029497,
            _ => throw new HttpStatusCodeException(StatusCodes.Status422UnprocessableEntity,
                "Invalid Unit Value. Unit must be either Kg, Lb or St.")
        };

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

public class PatientWeightPayload : IValidatableObject
{
    public double Weight { get; init; }

    public string Unit { get; init; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var unitIsValid = Enum.TryParse(Unit, out WeightUnit _);

        if (Weight <= 0.0)
        {
            yield return new ValidationResult(
                "Weight value must be higher than 0.",
                new[] { nameof(Weight) });
        }

        if (!unitIsValid)
        {
            yield return new ValidationResult(
                "Invalid Unit Value. Unit must be either Kg, Lb or St.",
                new[] { nameof(Unit) });
        }
    }

    public static bool TryParse(PatientWeightFormValues formValues, out PatientWeightPayload payload)
    {
        if (formValues.Weight > 0.0)
        {
            payload = new PatientWeightPayload
            {
                Weight = formValues.Weight.Value,
                Unit = formValues.Unit.ToString()
            };
            return true;
        }

        payload = new();
        return false;
    }
}

public class PatientWeightFormValues
{
    public double? Weight { get; set; }

    public WeightUnit Unit { get; set; } = WeightUnit.Lb;

    public void Reset()
    {
        Weight = null;
        Unit = WeightUnit.Lb;
    }
}