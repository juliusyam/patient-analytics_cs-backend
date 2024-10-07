using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PatientAnalytics.Middleware;
using PatientAnalytics.Controllers.PatientMetrics;
using PatientAnalytics.Controllers;
using Microsoft.AspNetCore.Http;
using NUnit.Framework.Legacy;
using PatientAnalytics.Models;

namespace PatientAnalytics.Tests.PatientMetrics;

[TestFixture]
public class PatientMetricsTemperatureControllerTest : TemperatureBaseTest
{
    [Test]
    public async Task CreateTemperatureEntry_WithLoggedInDoctorCelsius_SuccessfulCreation()
    {
        var response = await PatientMetricsTemperatureController.CreateEntry(DoctorHttpContextAccessor, _patientOne.Id, TempCelsiusPayload);

        Assert.That(response!.TemperatureCelsius, Is.EqualTo(TempCelsiusPayload.Temperature));
    }

    [Test]
    public async Task CreateTemperatureEntry_WithLoggedInDoctorFahrenheit_SuccessfulCreation()
    {
        var response = await PatientMetricsTemperatureController.CreateEntry(DoctorHttpContextAccessor, _patientOne.Id, TempFahrenheitPayload);

        Assert.That(response!.TemperatureFahrenheit, Is.EqualTo(TempFahrenheitPayload.Temperature));
    }

    [Test]
    public async Task CreateTemperatureEntry_WithInvalidUnit_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        var pat = await PatientController.CreatePatient(DoctorHttpContextAccessor, PersonPayload);

        async Task Action() => await PatientMetricsTemperatureController.CreateEntry(DoctorHttpContextAccessor, pat.Id, TempWrongUnitPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo("Invalid Unit Value. Unit must be either Celsius or Fahrenheit."));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status422UnprocessableEntity));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateTemperatureEntry_WithSuperAdmin_Unauthorized_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsTemperatureController.CreateEntry(SuperAdminHttpContextAccessor, _patientOne.Id, TempCelsiusPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateTemperatureEntry_WithAdmin_Unauthorized_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsTemperatureController.CreateEntry(AdminHttpContextAccessor, _patientOne.Id, TempCelsiusPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateTemperatureEntry_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsTemperatureController.CreateEntry(DoctorHttpContextAccessor, FakePatientId, TempCelsiusPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void CreateTemperatureEntry_NotDoctorsPatient_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsTemperatureController.CreateEntry(DoctorHttpContextAccessor, _patientZero.Id, TempCelsiusPayload);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {_patientZero.Id}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public async Task GetTemperature_WithLoggedInDoctor_SuccessfulCase()
    {
        var doctorTemperatureList = await PatientMetricsTemperatureController.GetPatientTemperatures(DoctorHttpContextAccessor, _patientOne.Id);
        var patientTemperatureList = DbContext.PatientTemperatures
                                  .Where(bp => bp.PatientId == _patientOne.Id)
                                  .ToList();

        Assert.That(doctorTemperatureList!.Count, Is.EqualTo(DbContext.PatientTemperatures.Count()));
        CollectionAssert.AreEquivalent(doctorTemperatureList, patientTemperatureList);
    }

    [Test]
    public void GetTemperature_WithSuperAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsTemperatureController.GetPatientTemperatures(SuperAdminHttpContextAccessor, _patientOne.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetTemperature_WithAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsTemperatureController.GetPatientTemperatures(AdminHttpContextAccessor, _patientOne.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetTemperature_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsTemperatureController.GetPatientTemperatures(DoctorHttpContextAccessor, FakePatientId);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetTemperature_NotDoctorsPatient_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        async Task Action() => await PatientMetricsTemperatureController.GetPatientTemperatures(DoctorHttpContextAccessor, _patientZero.Id);

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {_patientZero.Id}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetTemperatureEntryById_WithLoggedInDoctor_SuccessfulCase()
    {
        var PatientTemperature = PatientMetricsTemperatureController.GetEntryById(DoctorHttpContextAccessor, _patientTemperature01.Id);

        Assert.That(PatientTemperature.Id, Is.EqualTo(_patientTemperature01.Id));
        Assert.That(PatientTemperature.PatientId, Is.EqualTo(_patientTemperature01.PatientId));
        Assert.That(PatientTemperature.DoctorId, Is.EqualTo(_patientTemperature01.DoctorId));
        Assert.That(PatientTemperature.TemperatureCelsius, Is.EqualTo(_patientTemperature01.TemperatureCelsius));
        Assert.That(PatientTemperature.TemperatureFahrenheit, Is.EqualTo(_patientTemperature01.TemperatureFahrenheit));
    }

    [Test]
    public void GetTemperatureEntryById_WithSuperAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsTemperatureController.GetEntryById(SuperAdminHttpContextAccessor, _patientTemperature01.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetTemperatureEntryById_WithAdmin_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsTemperatureController.GetEntryById(AdminHttpContextAccessor, _patientTemperature01.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public void GetTemperatureEntryById_PatientNotFound_FailedCase()
    {
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsTemperatureController.GetEntryById(DoctorHttpContextAccessor, FakePatientId);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"Unable to locate temperature record with id: {FakePatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }

    [Test]
    public async Task GetTemperatureEntryById_NotDoctorsPatient_FailedCase()
    {
        var patient = await PatientController.CreatePatient(Doctor02HttpContextAccessor, PersonPayload);
        var BP01 = await PatientMetricsTemperatureController.CreateEntry(Doctor02HttpContextAccessor, patient.Id, TempCelsiusPayload);
        var initialCount = DbContext.PatientBloodPressures.Count();

        void Action() => PatientMetricsTemperatureController.GetEntryById(DoctorHttpContextAccessor, BP01.Id);

        var exception = Assert.Throws<HttpStatusCodeException>(Action);

        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {BP01.PatientId}"));
        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var finalCount = DbContext.PatientBloodPressures.Count();

        Assert.That(finalCount, Is.EqualTo(initialCount));
    }
}
