using System.Linq;
using NUnit.Framework;
using PatientAnalytics.Controllers.PatientMetrics;
using PatientAnalytics.Models.PatientMetrics;
using PatientAnalytics.Services.PatientMetrics;

namespace PatientAnalytics.Tests;

public abstract class WeightBaseTest : PatientBaseTest
{
    private static readonly PatientMetricsWeightService PatientMetricsWeightService = new(DbContext, PatientService, Localized);
    protected static readonly PatientMetricsWeightController PatientMetricsWeightController = new(PatientMetricsWeightService, Localized);
    protected PatientWeight _patientWeight01;
    protected PatientWeight _patientWeight02;
    protected PatientWeight _patientWeight03;

    [SetUp]
    public void WeightSetUp()
    {
        _patientWeight01 = CreateTestWeight(_patientOne.Id, DoctorUser.Id, WeightKgPayload);
        AddPatientWeightSaveChanges(_patientWeight01);
        _patientWeight02 = CreateTestWeight(_patientOne.Id, DoctorUser.Id, WeightStPayload);
        AddPatientWeightSaveChanges(_patientWeight02);
        _patientWeight03 = CreateTestWeight(_patientOne.Id, DoctorUser.Id, WeightLbPayload);
        AddPatientWeightSaveChanges(_patientWeight03);
    }

    [TearDown]
    public void WeightTearDown()
    {
        ClearPatientsWeights();
    }

    protected static readonly PatientWeightPayload WeightKgPayload = new PatientWeightPayload(55, "Kg");
    protected static readonly PatientWeightPayload WeightStPayload = new PatientWeightPayload(11, "St");
    protected static readonly PatientWeightPayload WeightLbPayload = new PatientWeightPayload(160, "Lb");

    protected static readonly PatientWeightPayload WeightWrongUnitPayload = new PatientWeightPayload(6, "Ft");

    private static PatientWeight CreateTestWeight(int patientId, int doctorId, PatientWeightPayload payload)
    {
        // Define and return the patient weight for this test class
        return PatientWeight.CreateFromPayload(patientId, doctorId, payload);
    }

    private static void AddPatientWeightSaveChanges(PatientWeight newWeight)
    {
        DbContext.PatientWeights.Add(newWeight);
        DbContext.SaveChanges();
    }

    private static void ClearPatientsWeights()
    {
        var allPatients = DbContext.PatientWeights.ToList();
        foreach (var weights in allPatients)
        {
            DbContext.PatientWeights.Remove(weights);
        }
        DbContext.SaveChanges();
    }
}
