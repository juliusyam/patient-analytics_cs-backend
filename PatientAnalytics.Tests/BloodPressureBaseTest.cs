using NUnit.Framework;
using PatientAnalytics.Controllers.PatientMetrics;
using PatientAnalytics.Models.PatientMetrics;
using PatientAnalytics.Services.PatientMetrics;
using System.Linq;

namespace PatientAnalytics.Tests;

public abstract class BloodPressureBaseTest : PatientBaseTest
{
    private static readonly PatientMetricsBloodPressureService PatientMetricsBloodPressureService = new(DbContext, PatientService, Localized);
    protected static readonly PatientMetricsBloodPressureController PatientMetricsBloodPressureController = new(PatientMetricsBloodPressureService, Localized);
    protected PatientBloodPressure _patientBloodPressure01;
    protected PatientBloodPressure _patientBloodPressure02;

    [SetUp]
    public void BloodPressureSetUp()
    {
        _patientBloodPressure01 = CreateTestBloodPressure(_patientOne.Id, DoctorUser.Id, BloodPressurePayload);
        AddPatientBloodPressuretSaveChanges(_patientBloodPressure01);
        _patientBloodPressure02 = CreateTestBloodPressure(_patientOne.Id, DoctorUser.Id, BloodPressurePayload2);
        AddPatientBloodPressuretSaveChanges(_patientBloodPressure02);
    }

    [TearDown]
    public void BloodPressureTearDown()
    {
        ClearPatientsBloodPressures();
    }

    protected static readonly PatientBloodPressurePayload BloodPressurePayload = new()
    {
        BloodPressureSystolic = 120,
        BloodPressureDiastolic = 80
    };

    protected static readonly PatientBloodPressurePayload BloodPressurePayload2 = new PatientBloodPressurePayload()
    {
        BloodPressureSystolic = 180,
        BloodPressureDiastolic = 40
    };
    
    private static PatientBloodPressure CreateTestBloodPressure(int patientId, int doctorId, PatientBloodPressurePayload payload)
    {
        // Define and return the patient blood pressure for this test class
        return PatientBloodPressure.CreateFromPayload(patientId, doctorId, payload);
    }

    private static void AddPatientBloodPressuretSaveChanges(PatientBloodPressure newBloodPressure)
    {
        DbContext.PatientBloodPressures.Add(newBloodPressure);
        DbContext.SaveChanges();
    }

    private static void ClearPatientsBloodPressures()
    {
        var allPatients = DbContext.PatientHeights.ToList();
        foreach (var bloodpressures in allPatients)
        {
            DbContext.PatientHeights.Remove(bloodpressures);
        }
        DbContext.SaveChanges();
    }
}
