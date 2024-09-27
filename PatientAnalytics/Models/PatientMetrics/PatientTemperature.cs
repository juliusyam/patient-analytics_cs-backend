using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PatientAnalytics.Middleware;

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
    public double TemperatureFahrenheit { get; protected set; }

    public static PatientTemperature CreateFromPayload(int patientId, int doctorId, PatientTemperaturePayload payload)
    {
        double temperatureCelsius;
        
        switch (payload.Unit)
        {
            case "Celsius":
                temperatureCelsius = payload.Temperature;
                break;
            case "Fahrenheit":
                temperatureCelsius = (payload.Temperature - 32) / 1.8;
                break;
            default:
                throw new HttpStatusCodeException(StatusCodes.Status422UnprocessableEntity, 
                    "Invalid Unit Value. Unit must be either Celsius or Fahrenheit.");
        }

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
        TemperatureFahrenheit = (TemperatureCelsius * 1.8) + 32;
        return this;
    }
}

public class PatientTemperaturePayload
{ 
    public double Temperature { get; set; }

    [RegularExpression("^Celsius$|^Fahrenheit$",
        ErrorMessage = "Invalid Unit Value. Unit must be either Celsius or Fahrenheit.")]
    public string Unit { get; set; } = null!;

    public void SetTemperature(double temperature, string unit)
    {
        Temperature = temperature;
        Unit = unit;
    }
}