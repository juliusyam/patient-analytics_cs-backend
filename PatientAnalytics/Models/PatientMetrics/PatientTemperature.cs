using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models.PatientMetrics.Units;

namespace PatientAnalytics.Models.PatientMetrics;

public class PatientTemperature
{
    public int Id { get; protected set; }
    public int PatientId { get; protected set; }
    public int DoctorId { get; protected set; }

    [ForeignKey("DoctorId")]
    public User? Doctor { get; protected set; }

    [ForeignKey("PatientId")]
    public Patient? Patient { get; protected set; }
    
    public DateTime DateCreated { get; protected set; }
    public double TemperatureCelsius { get; protected set; }

    [NotMapped] 
    public string? TemperatureCelsiusFormatted { get; protected set; }

    [NotMapped] 
    public string? TemperatureFahrenheitFormatted { get; protected set; }

    public static PatientTemperature CreateFromPayload(int patientId, int doctorId, PatientTemperaturePayload payload)
    {
        Enum.TryParse(payload.Unit, out TemperatureUnit unit);

        var temperatureCelsius = unit switch
        {
            TemperatureUnit.Celsius => payload.Temperature,
            TemperatureUnit.Fahrenheit => (payload.Temperature - 32) / 1.8,
            _ => throw new HttpStatusCodeException(StatusCodes.Status422UnprocessableEntity,
                "Invalid Unit Value. Unit must be either Celsius or Fahrenheit.")
        };

        return new PatientTemperature
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DateCreated = DateTime.UtcNow,
            TemperatureCelsius = temperatureCelsius
        };
    }

    public PatientTemperature Formatted()
    {
        TemperatureCelsiusFormatted = TemperatureCelsius.ToString("0.##");

        TemperatureFahrenheitFormatted = (TemperatureCelsius * 1.8 + 32).ToString("0.##");

        return this;
    }
}

public class PatientTemperaturePayload : IValidatableObject
{
    public double Temperature { get; init; }

    public string Unit { get; init; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var unitIsValid = Enum.TryParse(Unit, out TemperatureUnit _);

        if (Temperature <= 0.0)
        {
            yield return new ValidationResult(
                "Temperature value must be higher than 0.",
                new[] { nameof(Temperature) });
        }

        if (!unitIsValid)
        {
            yield return new ValidationResult(
                "Invalid Unit Value. Unit must be either Celsius or Fahrenheit.",
                new[] { nameof(Unit) });
        }
    }

    public static bool TryParse(PatientTemperatureFormValues formValues, out PatientTemperaturePayload payload)
    {
        if (formValues.Temperature > 0.0)
        {
            payload = new PatientTemperaturePayload
            {
                Temperature = formValues.Temperature.Value,
                Unit = formValues.Unit.ToString()
            };
            return true;
        }

        payload = new();
        return false;
    }
}

public class PatientTemperatureFormValues
{
    public double? Temperature { get; set; }

    public TemperatureUnit Unit { get; set; } = TemperatureUnit.Celsius;

    public void Reset()
    {
        Temperature = null;
        Unit = TemperatureUnit.Celsius;
    }
}