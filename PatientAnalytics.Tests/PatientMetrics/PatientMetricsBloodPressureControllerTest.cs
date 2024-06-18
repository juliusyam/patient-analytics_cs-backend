using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PatientAnalytics.Middleware;
using PatientAnalytics.Controllers.PatientMetrics;
using PatientAnalytics.Controllers;
using Microsoft.AspNetCore.Http;
using NUnit.Framework.Legacy;

namespace PatientAnalytics.Tests.PatientMetrics;

[TestFixture]
public class PatientMetricsBloodPressureControllerTest : BloodPressureBaseTest
{
    [Test]
    public async Task CreateEntry_WithLoggedInDoctor_SuccessfulCreation()
    {
        var response = await PatientMetricsBloodPressureController.CreateEntry(DoctorHttpContextAccessor, _patientOne.Id, BloodPressurePayload);

        var patients = DbContext.PatientBloodPressures.ToList();

        var savedEntry = patients.Find(x => x.Id == response.Id);

        Assert.That(response!.BloodPressureSystolic, Is.EqualTo(BloodPressurePayload.BloodPressureSystolic));
        Assert.That(response!.BloodPressureDiastolic, Is.EqualTo(BloodPressurePayload.BloodPressureDiastolic));
        Assert.That(response!.BloodPressureSystolic, Is.EqualTo(savedEntry.BloodPressureSystolic));
        Assert.That(response!.BloodPressureDiastolic, Is.EqualTo(savedEntry.BloodPressureDiastolic));
    }

    [Test]
    public void CreateEntry_WithSuperAdmin_Unauthorized_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsBloodPressureController.CreateEntry(SuperAdminHttpContextAccessor, _patientOne.Id, BloodPressurePayload);
                
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateEntry_WithAdmin_Unauthorized_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsBloodPressureController.CreateEntry(AdminHttpContextAccessor, _patientOne.Id, BloodPressurePayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateEntry_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsBloodPressureController.CreateEntry(DoctorHttpContextAccessor, FakePatientId, BloodPressurePayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateEntry_NotDoctorsPatient_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsBloodPressureController.CreateEntry(DoctorHttpContextAccessor, _patientZero.Id, BloodPressurePayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {_patientZero.Id}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public async Task GetBloodPressures_WithLoggedInDoctor_SuccessfulCase()
    {
        var doctorPatientBPList = await PatientMetricsBloodPressureController.GetPatientBloodPressures(DoctorHttpContextAccessor, _patientOne.Id);
        var patientBPEntries = DbContext.PatientBloodPressures
                                  .Where(bp => bp.PatientId == _patientOne.Id)
                                  .ToList();
        
        CollectionAssert.AreEquivalent(doctorPatientBPList, patientBPEntries);
    }

    [Test]
    public void GetBloodPressures_WithSuperAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsBloodPressureController.GetPatientBloodPressures(SuperAdminHttpContextAccessor, _patientOne.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetBloodPressures_WithAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsBloodPressureController.GetPatientBloodPressures(AdminHttpContextAccessor, _patientOne.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetBloodPressures_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsBloodPressureController.GetPatientBloodPressures(DoctorHttpContextAccessor, FakePatientId);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetBloodPressures_NotDoctorsPatient_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsBloodPressureController.GetPatientBloodPressures(DoctorHttpContextAccessor, _patientZero.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {_patientZero.Id}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetEntryById_WithLoggedInDoctor_SuccessfulCase()
    {
        var PatientBP = PatientMetricsBloodPressureController.GetEntryById(DoctorHttpContextAccessor, _patientBloodPressure01.Id);

        Assert.That(PatientBP.Id, Is.EqualTo(_patientBloodPressure01.Id));
        Assert.That(PatientBP.PatientId, Is.EqualTo(_patientBloodPressure01.PatientId));
        Assert.That(PatientBP.DoctorId, Is.EqualTo(_patientBloodPressure01.DoctorId));
        Assert.That(PatientBP.BloodPressureSystolic, Is.EqualTo(_patientBloodPressure01.BloodPressureSystolic));
        Assert.That(PatientBP.BloodPressureDiastolic, Is.EqualTo(_patientBloodPressure01.BloodPressureDiastolic));
    }

    [Test]
    public void GetEntryById_WithSuperAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsBloodPressureController.GetEntryById(SuperAdminHttpContextAccessor, _patientBloodPressure01.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetEntryById_WithAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsBloodPressureController.GetEntryById(AdminHttpContextAccessor, _patientBloodPressure01.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetEntryById_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsBloodPressureController.GetEntryById(DoctorHttpContextAccessor, FakePatientId);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"Unable to locate blood pressure record with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public async Task GetEntryById_NotDoctorsPatient_FailedCase()
    {
        var pat = await PatientController.CreatePatient(Doctor02HttpContextAccessor, PersonPayload);
        var BP01 = await PatientMetricsBloodPressureController.CreateEntry(Doctor02HttpContextAccessor, pat.Id, BloodPressurePayload);

        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsBloodPressureController.GetEntryById(DoctorHttpContextAccessor, BP01.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {BP01.PatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }
}


