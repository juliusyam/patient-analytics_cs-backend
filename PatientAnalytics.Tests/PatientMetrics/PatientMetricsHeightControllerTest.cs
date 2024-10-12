using System.Threading.Tasks;
using NUnit.Framework;
using PatientAnalytics.Middleware;
using PatientAnalytics.Controllers.PatientMetrics;
using PatientAnalytics.Controllers;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using NUnit.Framework.Legacy;
using PatientAnalytics.Models;

namespace PatientAnalytics.Tests.PatientMetrics;

[TestFixture]
public class PatientMetricsHeightControllerTest : HeightBaseTest
{
    [Test]
    public async Task CreateHeightEntry_WithLoggedInDoctorCm_SuccessfulCreation()
    {
        var response = await PatientMetricsHeightController.CreateEntry(DoctorHttpContextAccessor, _patientOne.Id, HeightCmPayload);
        var patients = DbContext.PatientHeights.ToList();
        var savedEntry = patients.Find(x => x.Id == response.Id);

        Assert.That(response!.HeightCm, Is.EqualTo(HeightCmPayload.Height));
        Assert.That(response!.HeightCm, Is.EqualTo(savedEntry.HeightCm));
    }

    [Test]
    public async Task CreateHeightEntry_WithLoggedInDoctorIn_SuccessfulCreation()
    {
        var response = await PatientMetricsHeightController.CreateEntry(DoctorHttpContextAccessor, _patientOne.Id, HeightInPayload);
        var patients = DbContext.PatientHeights.ToList();
        var savedEntry = patients.Find(x => x.Id == response.Id);

        Assert.That(response!.HeightInFormatted, Is.EqualTo(HeightInPayload.Height.ToString("0.##")));
        Assert.That(response!.HeightInFormatted, Is.EqualTo(savedEntry.HeightInFormatted));
    }

    [Test]
    public void CreateHeightEntry_WithInvalidUnit_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsHeightController.CreateEntry(DoctorHttpContextAccessor, _patientOne.Id, HeightWrongUnitPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo("Invalid Unit Value. Unit must be either Cm or In."));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status422UnprocessableEntity));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateHeightEntry_WithSuperAdmin_Unauthorized_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsHeightController.CreateEntry(SuperAdminHttpContextAccessor, _patientOne.Id, HeightCmPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateHeightEntry_WithAdmin_Unauthorized_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsHeightController.CreateEntry(AdminHttpContextAccessor, _patientOne.Id, HeightCmPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateHeightEntry_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsHeightController.CreateEntry(DoctorHttpContextAccessor, FakePatientId, HeightCmPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateHeightEntry_NotDoctorsPatient_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsHeightController.CreateEntry(DoctorHttpContextAccessor, _patientZero.Id, HeightCmPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {_patientZero.Id}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public async Task GetHeight_WithLoggedInDoctor_SuccessfulCase()
    {
        var doctorPatientHeightList = await PatientMetricsHeightController.GetPatientHeights(DoctorHttpContextAccessor, _patientOne.Id);
        var patientHeightList = DbContext.PatientHeights
                                  .Where(bp => bp.PatientId == _patientOne.Id)
                                  .ToList();

        CollectionAssert.AreEquivalent(doctorPatientHeightList, patientHeightList);
    }

    [Test]
    public void GetHeight_WithSuperAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsHeightController.GetPatientHeights(SuperAdminHttpContextAccessor, _patientOne.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetHeight_WithAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsHeightController.GetPatientHeights(AdminHttpContextAccessor, _patientOne.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetHeight_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsHeightController.GetPatientHeights(DoctorHttpContextAccessor, FakePatientId);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetHeight_NotDoctorsPatient_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsHeightController.GetPatientHeights(DoctorHttpContextAccessor, _patientZero.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {_patientZero.Id}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetHeightEntryById_WithLoggedInDoctor_SuccessfulCase()
    {
        var PatientHeight = PatientMetricsHeightController.GetEntryById(DoctorHttpContextAccessor, _patientHeight01.Id);

        Assert.That(PatientHeight.Id, Is.EqualTo(_patientHeight01.Id));
        Assert.That(PatientHeight.PatientId, Is.EqualTo(_patientHeight01.PatientId));
        Assert.That(PatientHeight.DoctorId, Is.EqualTo(_patientHeight01.DoctorId));
        Assert.That(PatientHeight.HeightCm, Is.EqualTo(_patientHeight01.HeightCm));
        Assert.That(PatientHeight.HeightInFormatted, Is.EqualTo(_patientHeight01.HeightInFormatted));
        Assert.That(PatientHeight.HeightFtFormatted, Is.EqualTo(_patientHeight01.HeightFtFormatted));
    }

    [Test]
    public void GetHeightEntryById_WithSuperAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsHeightController.GetEntryById(SuperAdminHttpContextAccessor, _patientHeight01.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetHeightEntryById_WithAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsHeightController.GetEntryById(AdminHttpContextAccessor, _patientHeight01.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetHeightEntryById_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsHeightController.GetEntryById(DoctorHttpContextAccessor, FakePatientId);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"Unable to locate height record with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public async Task GetHeightEntryById_NotDoctorsPatient_FailedCase()
    {
        var patient = await PatientController.CreatePatient(Doctor02HttpContextAccessor, PersonPayload);
        var heightEntry = await PatientMetricsHeightController.CreateEntry(Doctor02HttpContextAccessor, patient.Id, HeightCmPayload);
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsHeightController.GetEntryById(DoctorHttpContextAccessor, heightEntry.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {heightEntry.PatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }
}
