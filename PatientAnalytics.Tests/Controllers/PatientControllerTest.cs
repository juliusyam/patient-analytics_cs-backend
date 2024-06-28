using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using PatientAnalytics.Middleware;

namespace PatientAnalytics.Tests.Controllers;

[TestFixture]
public class PatientControllerTest : PatientBaseTest
{
    [Test]
    public async Task CreatePatient_WithLoggedInDoctor_SuccessfulCreation()
    {
        var prePatientCount = DbContext.Patients.Count();
        var response = await PatientController.CreatePatient(DoctorHttpContextAccessor, PersonPayload);
        var postPatientCount = DbContext.Patients.Count();

        Assert.That(postPatientCount, Is.EqualTo(prePatientCount + 1));
        Assert.That(response!.FirstName, Is.EqualTo(PersonPayload.FirstName));
        Assert.That(response.LastName, Is.EqualTo(PersonPayload.LastName));
    }

    [Test]
    public void CreatePatient_WithLoggedInSuperAdmin_ThrowException()
    {
        async Task Action() => await PatientController.CreatePatient(SuperAdminHttpContextAccessor, PersonPayload);
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
    }

    [Test]
    public void CreatePatient_WithLoggedInAdmin_ThrowException()
    {
        async Task Action() => await PatientController.CreatePatient(AdminHttpContextAccessor, PersonPayload);
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
    }

    [Test]
    public void CreatePatient_WithNotLoggedInUser_ThrowsMessage()
    {        
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(async () =>
        {
            await PatientController.CreatePatient(CreateHttpContextAccessor("Bob"), PersonPayload);
        });

        Assert.That(exception!.Message, Is.EqualTo("Token format is invalid"));
    }

    [Test]
    public async Task EditPatient_WithLoggedInDoctor_SuccessfullyEditPatient()
    {
        var response = await PatientController.CreatePatient(DoctorHttpContextAccessor, PersonPayload);

        await PatientController.EditPatient(DoctorHttpContextAccessor, response!.Id, UpdatedPersonPayload);

        Assert.That(response.Address, Is.Not.EqualTo(PersonPayload.Address));
        Assert.That(response.Address, Is.EqualTo(UpdatedPersonPayload.Address));
    }

    [Test]
    public async Task EditPatient_WrongPatientId_ThrowException()
    {
        await PatientController.CreatePatient(DoctorHttpContextAccessor, PersonPayload);
        
        async Task Action() => await PatientController.EditPatient(DoctorHttpContextAccessor, FakePatientId, UpdatedPersonPayload);
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {FakePatientId}"));
    }

    [Test]
    public async Task EditPatient_NotDoctorsPatient_ThrowException()
    {
        await PatientController.CreatePatient(DoctorHttpContextAccessor, PersonPayload);

        var patients = DbContext.Patients.ToList();

        async Task Action() => await PatientController.EditPatient(DoctorHttpContextAccessor, patients[0].Id, UpdatedPersonPayload);
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {patients[0].Id}"));
    }

    [Test]
    public async Task EditPatient_EmailAlreadyTaken_ThrowException()
    {
        await PatientController.CreatePatient(DoctorHttpContextAccessor, PersonPayload);

        var patients = DbContext.Patients.ToList();

        async Task Action() => await PatientController.EditPatient(DoctorHttpContextAccessor, patients[1].Id, UpdatedPersonPayload02);
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(exception.Message, Is.EqualTo($"Email address {UpdatedPersonPayload02.Email} already taken"));
    }

    [Test]
    public void EditPatient_WithNotLoggedInUser_ThrowsMessage()
    {
        var patients = DbContext.Patients.ToList();

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(async () =>
        {
            await PatientController.DeletePatient(CreateHttpContextAccessor("Bob"), patients[0].Id);
        });

        Assert.That(exception!.Message, Is.EqualTo("Token format is invalid"));
    }

    [Test]
    public void EditPatient_SuperUser_ThrowsException()
    {
        var patients = DbContext.Patients.ToList();

        async Task Action() => await PatientController.EditPatient(SuperAdminHttpContextAccessor, patients[0].Id, UpdatedPersonPayload); 
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public void EditPatient_AdminUser_ThrowsException()
    {
        var patients = DbContext.Patients.ToList();

        async Task Action() => await PatientController.EditPatient(SuperAdminHttpContextAccessor, patients[0].Id, UpdatedPersonPayload); 
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public async Task GetPatientById_WithLoggedInDoctor_SuccessfullyRetrievePatient()
    {
        var response = await PatientController.CreatePatient(DoctorHttpContextAccessor, PersonPayload);
        
        var patient = PatientController.GetPatientById(DoctorHttpContextAccessor, response!.Id);

        Assert.That(patient, Is.EqualTo(response));
    }

    [Test]
    public void GetPatientById_NotDoctorsPatient_ThrowException()
    {
        var patients = DbContext.Patients.ToList();

        Task Action() { PatientController.GetPatientById(DoctorHttpContextAccessor, patients[0].Id); return Task.CompletedTask; }
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {patients[0].Id}"));
    }

    [Test]
    public void GetPatientById_WithNotLoggedInUser_ThrowsMessage()
    {
        var patients = DbContext.Patients.ToList();

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(() => {
            PatientController.GetPatientById(CreateHttpContextAccessor("Bob"), patients[0].Id);
            return Task.CompletedTask;
        });

        Assert.That(exception!.Message, Is.EqualTo("Token format is invalid"));
    }

    [Test]
    public void GetPatientById_SuperUser_ThrowsException()
    {
        var patients = DbContext.Patients.ToList();

        Task Action() { PatientController.GetPatientById(SuperAdminHttpContextAccessor, patients[0].Id); return Task.CompletedTask; }
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public void GetPatientById_AdminUser_ThrowsException()
    {
        var patients = DbContext.Patients.ToList();

        Task Action() { PatientController.GetPatientById(AdminHttpContextAccessor, patients[0].Id); return Task.CompletedTask; }
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public void DeletePatient_NotDoctorsPatient_ThrowException()
    {
        var patients = DbContext.Patients.ToList();

        async Task Action() => await PatientController.DeletePatient(DoctorHttpContextAccessor, patients[0].Id);
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from viewing and modifying patient with id: {patients[0].Id}"));
    }

    [Test]
    public void DeletePatient_WrongPatientId_ThrowException()
    {
        async Task Action() => await PatientController.DeletePatient(DoctorHttpContextAccessor, FakePatientId);
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {FakePatientId}"));
    }

    [Test]
    public void DeletePatient_WithNotLoggedInUser_ThrowsMessage()
    {
        var patients = DbContext.Patients.ToList();
        
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(async () =>
        {
            await PatientController.DeletePatient(CreateHttpContextAccessor("Bob"), patients[0].Id);
        });

        Assert.That(exception!.Message, Is.EqualTo("Token format is invalid"));
    }

    [Test]
    public void DeletePatient_SuperUser_ThrowsException()
    {
        var patients = DbContext.Patients.ToList();

        async Task Action() => await PatientController.DeletePatient(SuperAdminHttpContextAccessor, patients[0].Id);  
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public void DeletePatient_AdminUser_ThrowsException()
    {
        var patients = DbContext.Patients.ToList();

        async Task Action() => await PatientController.DeletePatient(AdminHttpContextAccessor, patients[0].Id);
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public async Task GetPatients_WithLoggedInDoctor_SuccessfulFindPatient()
    {
        var response = await PatientController.CreatePatient(DoctorHttpContextAccessor, PersonPayload);

        var results = PatientController.GetPatients(DoctorHttpContextAccessor, response!.Email, response.FirstName + " " + response.LastName, response.Address);
        
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Email, Is.EqualTo(response.Email));
        Assert.That(results[0].FirstName, Is.EqualTo(response.FirstName));
        Assert.That(results[0].LastName, Is.EqualTo(response.LastName));
    }

    [Test]
    public async Task GetPatients_UsingOnlyToken_SuccessfulListPatients()
    {
        await PatientController.CreatePatient(DoctorHttpContextAccessor, PersonPayload);
        await PatientController.CreatePatient(DoctorHttpContextAccessor, PersonPayload02);

        var results = PatientController.GetPatients(DoctorHttpContextAccessor, null, null, null);
        foreach (var result in results)
        {
            Assert.That(result.DoctorId, Is.EqualTo(DoctorUser.Id));
        }
    }

    [Test]
    public void GetPatients_WithNotLoggedInUser_ThrowsMessage()
    {
        var patients = DbContext.Patients.ToList();

        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(() => {
            PatientController.GetPatientById(CreateHttpContextAccessor("Bob"), patients[0].Id);
            return Task.CompletedTask;
        });

        Assert.That(exception!.Message, Is.EqualTo("Token format is invalid"));
    }

    [Test]
    public async Task GetPatients_DoctorsPatientsOnly_SuccessfullCreation()
    {
        var response = await PatientController.CreatePatient(DoctorHttpContextAccessor, PersonPayload);

        var results = PatientController.GetPatients(DoctorHttpContextAccessor, response!.Email, $"{response.FirstName} {response.LastName}", response.Address);
        
        Assert.That(results[0].DoctorId, Is.EqualTo(DoctorUser.Id));
    }

    [Test]
    public void GetPatients_SuperUser_ThrowsException()
    {
        Task Action() { PatientController.GetPatients(SuperAdminHttpContextAccessor, null, null, null); return Task.CompletedTask; }
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public void GetPatients_AdminUser_ThrowsException()
    {
        Task Action() { PatientController.GetPatients(AdminHttpContextAccessor, null, null, null); return Task.CompletedTask; }
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    // Unable to test DeletePatient method, because:
    // ExecuteUpdate and ExecuteDelete are not supported by the in-memory database.
}
