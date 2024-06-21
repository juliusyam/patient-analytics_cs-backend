using System.Linq;
using NUnit.Framework;
using PatientAnalytics.Controllers.PatientMetrics;
using PatientAnalytics.Models.PatientMetrics;
using PatientAnalytics.Services.PatientMetrics;

namespace PatientAnalytics.Tests;
public abstract class HeightBaseTest : PatientBaseTest
{
    private static readonly PatientMetricsHeightService PatientMetricsHeightService = new(DbContext, PatientService, Localized);
    protected static readonly PatientMetricsHeightController PatientMetricsHeightController = new(PatientMetricsHeightService, Localized);
    protected PatientHeight _patientHeight01;
    protected PatientHeight _patientHeight02;

    [SetUp]
    public void HeightSetUp()
    {
        _patientHeight01 = CreateTestHeight(_patientOne.Id, DoctorUser.Id, HeightCmPayload);
        AddPatientHeightSaveChanges(_patientHeight01);
        _patientHeight02 = CreateTestHeight(_patientOne.Id, DoctorUser.Id, HeightInPayload);
        AddPatientHeightSaveChanges(_patientHeight02);
    }

    [TearDown]
    public void HeightTearDown()
    {
        ClearPatientsHeights();
    }

    protected static readonly PatientHeightPayload HeightCmPayload = new()
    {
        Height = 180,
        Unit = "Cm"
    };

    protected static readonly PatientHeightPayload HeightInPayload = new()
    {
        Height = 50,
        Unit = "In"
    };

    protected static readonly PatientHeightPayload HeightWrongUnitPayload = new()
    {
        Height = 6,
        Unit = "Ft"
    };

    private static PatientHeight CreateTestHeight(int patientId, int doctorId, PatientHeightPayload payload)
    {
        // Define and return the patient height for this test class
        return PatientHeight.CreateFromPayload(patientId, doctorId, payload);
    }

    private static void AddPatientHeightSaveChanges(PatientHeight newHeight)
    {
        DbContext.PatientHeights.Add(newHeight);
        DbContext.SaveChanges();
    }

    private static void ClearPatientsHeights()
    {
        var allPatients = DbContext.PatientHeights.ToList();
        foreach (var heights in allPatients)
        {
            DbContext.PatientHeights.Remove(heights);
        }
        DbContext.SaveChanges();
    }
}


