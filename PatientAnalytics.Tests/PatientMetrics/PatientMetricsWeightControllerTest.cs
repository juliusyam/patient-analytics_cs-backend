using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PatientAnalytics.Middleware;
using PatientAnalytics.Controllers.PatientMetrics;
using PatientAnalytics.Controllers;
using Microsoft.AspNetCore.Http;
using NUnit.Framework.Legacy;
using PatientAnalytics.Models.PatientMetrics;

namespace PatientAnalytics.Tests.PatientMetrics;

[TestFixture]
public class PatientMetricsWeightControllerTest : WeightBaseTest
{
    [Test]
    public async Task CreateWeightEntry_WithLoggedInDoctorKg_SuccessfulCreation()
    {
        var response = await PatientMetricsWeightController.CreateEntry(DoctorHttpContextAccessor, _patientOne.Id, WeightKgPayload);
        var patients = DbContext.PatientWeights.ToList();
        var savedEntry = patients.Find(x => x.Id == response.Id);

        Assert.That(response!.WeightKg, Is.EqualTo(WeightKgPayload.Weight));
        Assert.That(response!.WeightKg, Is.EqualTo(savedEntry.WeightKg));
    }

    [Test]
    public async Task CreateWeightEntry_WithLoggedInDoctorSt_SuccessfulCreation()
    {
        var response = await PatientMetricsWeightController.CreateEntry(DoctorHttpContextAccessor, _patientOne.Id, WeightStPayload);
        var patients = DbContext.PatientWeights.ToList();
        var savedEntry = patients.Find(x => x.Id == response.Id);

        Assert.That(response!.WeightStFormatted, Is.EqualTo(WeightStPayload.Weight.ToString("0.##")));
        Assert.That(response!.WeightStFormatted, Is.EqualTo(savedEntry.WeightStFormatted).Within(0.1));
    }

    [Test]
    public async Task CreateWeightEntry_WithLoggedInDoctorLb_SuccessfulCreation()
    {
        var response = await PatientMetricsWeightController.CreateEntry(DoctorHttpContextAccessor, _patientOne.Id, WeightLbPayload);
        var patients = DbContext.PatientWeights.ToList();
        var savedEntry = patients.Find(x => x.Id == response.Id);
        
        Assert.That(response!.WeightLbFormatted, Is.EqualTo(WeightLbPayload.Weight.ToString("0.##")));
        Assert.That(response!.WeightLbFormatted, Is.EqualTo(savedEntry.WeightLbFormatted).Within(0.1));
    }

    [Test]
    public void CreateWeightEntry_WithInvalidUnit_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsWeightController.CreateEntry(DoctorHttpContextAccessor, _patientOne.Id, WeightWrongUnitPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo("Invalid Unit Value. Unit must be either Kg, Lb or St."));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status422UnprocessableEntity));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateWeightEntry_WithSuperAdmin_Unauthorized_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();
                
        async Task Action() => await PatientMetricsWeightController.CreateEntry(SuperAdminHttpContextAccessor, _patientOne.Id, WeightKgPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateWeightEntry_WithAdmin_Unauthorized_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsWeightController.CreateEntry(AdminHttpContextAccessor, _patientOne.Id, WeightKgPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateWeightEntry_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsWeightController.CreateEntry(DoctorHttpContextAccessor, FakePatientId, WeightKgPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateWeightEntry_NotDoctorsPatient_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsWeightController.CreateEntry(DoctorHttpContextAccessor, _patientZero.Id, WeightKgPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {_patientZero.Id}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public async Task GetWeight_WithLoggedInDoctor_SuccessfulCase()
    {
        var doctorPatientWeightList = await PatientMetricsWeightController.GetPatientWeights(DoctorHttpContextAccessor, _patientOne.Id);
        var patientWeightList = DbContext.PatientWeights
                                  .Where(bp => bp.PatientId == _patientOne.Id)
                                  .ToList();

        CollectionAssert.AreEquivalent(doctorPatientWeightList, patientWeightList);
    }

    [Test]
    public void GetWeight_WithSuperAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsWeightController.GetPatientWeights(SuperAdminHttpContextAccessor, _patientOne.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetWeight_WithAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsWeightController.GetPatientWeights(AdminHttpContextAccessor, _patientOne.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetWeight_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsWeightController.GetPatientWeights(DoctorHttpContextAccessor, FakePatientId);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetWeight_NotDoctorsPatient_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsWeightController.GetPatientWeights(DoctorHttpContextAccessor, _patientZero.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {_patientZero.Id}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetWeightEntryById_WithLoggedInDoctor_SuccessfulCase()
    {
        var PatientWeight = PatientMetricsWeightController.GetEntryById(DoctorHttpContextAccessor, _patientWeight01.Id);

        Assert.That(PatientWeight.Id, Is.EqualTo(_patientWeight01.Id));
        Assert.That(PatientWeight.PatientId, Is.EqualTo(_patientWeight01.PatientId));
        Assert.That(PatientWeight.DoctorId, Is.EqualTo(_patientWeight01.DoctorId));
        Assert.That(PatientWeight.WeightKg, Is.EqualTo(_patientWeight01.WeightKg));
        Assert.That(PatientWeight.WeightLbFormatted, Is.EqualTo(_patientWeight01.WeightLbFormatted));
        Assert.That(PatientWeight.WeightStFormatted, Is.EqualTo(_patientWeight01.WeightStFormatted));
    }

    [Test]
    public void GetWeightEntryById_WithSuperAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsWeightController.GetEntryById(SuperAdminHttpContextAccessor, _patientWeight01.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);
        
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetWeightEntryById_WithAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsWeightController.GetEntryById(AdminHttpContextAccessor, _patientWeight01.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetWeightEntryById_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsWeightController.GetEntryById(DoctorHttpContextAccessor, FakePatientId);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"Unable to locate weight record with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public async Task GetWeightEntryById_NotDoctorsPatient_FailedCase()
    {
        var patient = await PatientController.CreatePatient(Doctor02HttpContextAccessor, PersonPayload);
        var BP01 = await PatientMetricsWeightController.CreateEntry(Doctor02HttpContextAccessor, patient.Id, WeightKgPayload);
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsWeightController.GetEntryById(DoctorHttpContextAccessor, BP01.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {BP01.PatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }
}
