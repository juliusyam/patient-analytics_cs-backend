using System.Linq;
using NUnit.Framework;
using PatientAnalytics.Controllers.PatientMetrics;
using PatientAnalytics.Models.PatientMetrics;
using PatientAnalytics.Services.PatientMetrics;

namespace PatientAnalytics.Tests;

public abstract class TemperatureBaseTest : PatientBaseTest
{
    private static readonly PatientMetricsTemperatureService PatientMetricsTemperatureService = new(DbContext, PatientService, Localized);
    protected static readonly PatientMetricsTemperatureController PatientMetricsTemperatureController = new(PatientMetricsTemperatureService, Localized);
    protected PatientTemperature _patientTemperature01;
    protected PatientTemperature _patientTemperature02;

    [SetUp]
    public void TemperatureSetUp()
    {
        _patientTemperature01 = CreateTestTemperature(_patientOne.Id, DoctorUser.Id, TempCelsiusPayload);
        AddPatientTemperatureSaveChanges(_patientTemperature01);
        _patientTemperature02 = CreateTestTemperature(_patientOne.Id, DoctorUser.Id, TempFahrenheitPayload);
        AddPatientTemperatureSaveChanges(_patientTemperature02);
    }

    [TearDown]
    public void TemperatureTearDown()
    {
        ClearPatientsTemperatures();
    }

    protected static readonly PatientTemperaturePayload TempCelsiusPayload = new PatientTemperaturePayload(38, "Celsius");
    protected static readonly PatientTemperaturePayload TempFahrenheitPayload = new PatientTemperaturePayload(98, "Fahrenheit");

    protected static readonly PatientTemperaturePayload TempWrongUnitPayload = new PatientTemperaturePayload(60, "Ft");

    private static PatientTemperature CreateTestTemperature(int patientId, int doctorId, PatientTemperaturePayload payload)
    {
        // Define and return the patient temperature for this test class
        return PatientTemperature.CreateFromPayload(patientId, doctorId, payload);
    }

    private static void AddPatientTemperatureSaveChanges(PatientTemperature newTemperature)
    {
        DbContext.PatientTemperatures.Add(newTemperature);
        DbContext.SaveChanges();
    }

    private static void ClearPatientsTemperatures()
    {
        var allPatients = DbContext.PatientTemperatures.ToList();
        foreach (var temperatures in allPatients)
        {
            DbContext.PatientTemperatures.Remove(temperatures);
        }
        DbContext.SaveChanges();
    }
}
