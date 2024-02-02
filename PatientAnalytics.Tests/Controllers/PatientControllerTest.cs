using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using PatientAnalytics.Middleware;
using PatientAnalytics.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PatientAnalytics.Tests.Controllers;

[TestFixture]
public class PatientControllerTest : PatientBaseTest
{
    [SetUp]
    public void SetUp()
    {
        _superUser = CreateSuperUserForTest();
        AddSaveChanges(_superUser);

        _adminUser = CreateAdminUserForTest();
        AddSaveChanges(_adminUser);

        _doctorUser = CreateDoctorUserForTest();
        AddSaveChanges(_doctorUser);

        _patientZero = CreateNonUserPatientForTest();
        AddPatientSaveChanges(_patientZero);
    }

    [TearDown]
    public void TearDown()
    {
        ClearUsers();
        ClearPatients();
    }

    [Test]
    public async Task CreatePatient_WithLoggedInDoctor_SuccessfullCreation()
    {
        var token = JwtService.GenerateJwt(_doctorUser);

        var prePatientCount = DbContext.Patients.Count();
        var response = await _controller.CreatePatient(token, PersonPayload);
        var postPatientCount = DbContext.Patients.Count();

        Assert.That(postPatientCount, Is.EqualTo(prePatientCount + 1));
        Assert.That(response.FirstName, Is.EqualTo(PersonPayload.FirstName));
        Assert.That(response.LastName, Is.EqualTo(PersonPayload.LastName));
    }

    [Test]
    public void CreatePatient_WithLoggedInSuperAdmin_ThrowException()
    {
        var token = JwtService.GenerateJwt(_superUser);

        async Task Action() => await _controller.CreatePatient(token, PersonPayload);
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
    }

    [Test]
    public void CreatePatient_WithLoggedInAdmin_ThrowException()
    {
        var token = JwtService.GenerateJwt(_adminUser);

        async Task Action() => await _controller.CreatePatient(token, PersonPayload);
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
    }

    [Test]
    public void CreatePatient_WithNotLoggedInUser_ThrowsMessage()
    {        
        var exception = Assert.ThrowsAsync<SecurityTokenMalformedException>(async () =>
        {
            await _controller.CreatePatient("Bob", PersonPayload);
        });

        Assert.That(exception.Message, Is.EqualTo(_expectedTokenMessage));
    }

    [Test]
    public async Task EditPatient_WithLoggedInDoctor_SuccessfullyEditPatient()
    {
        var token = JwtService.GenerateJwt(_doctorUser);
        var response = await _controller.CreatePatient(token, PersonPayload);

        await _controller.EditPatient(token, response.Id, UpdatedPersonPayload);

        Assert.That(response.Address, Is.Not.EqualTo(PersonPayload.Address));
        Assert.That(response.Address, Is.EqualTo(UpdatedPersonPayload.Address));
    }

    [Test]
    public async Task EditPatient_WrongPatientId_ThrowException()
    {
        var token = JwtService.GenerateJwt(_doctorUser);

        await _controller.CreatePatient(token, PersonPayload);
        
        async Task Action() => await _controller.EditPatient(token, _fakePatientId, UpdatedPersonPayload);
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {_fakePatientId}"));
    }

    [Test]
    public async Task EditPatient_NotDoctorsPatient_ThrowException()
    {
        var token = JwtService.GenerateJwt(_doctorUser);
        await _controller.CreatePatient(token, PersonPayload);

        var patients = DbContext.Patients.ToList();

        async Task Action() => await _controller.EditPatient(token, patients[0].Id, UpdatedPersonPayload);
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(exception.Message, Is.EqualTo($"This user is forbidden from editing patient with id: {patients[0].Id}"));
    }

    [Test]
    public async Task EditPatient_EmailAlreadyTaken_ThrowException()
    {
        var token = JwtService.GenerateJwt(_doctorUser);

        await _controller.CreatePatient(token, PersonPayload);

        var patients = DbContext.Patients.ToList();

        async Task Action() => await _controller.EditPatient(token, patients[1].Id, UpdatedPersonPayload02);
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(exception.Message, Is.EqualTo($"Email address {UpdatedPersonPayload02.Email} already taken"));
    }

    [Test]
    public void EditPatient_WithNotLoggedInUser_ThrowsMessage()
    {
        var token = JwtService.GenerateJwt(_superUser);

        var patients = DbContext.Patients.ToList();

        var exception = Assert.ThrowsAsync<SecurityTokenMalformedException>(async () =>
        {
            await _controller.DeletePatient("Bob", patients[0].Id);
        });

        Assert.That(exception.Message, Is.EqualTo(_expectedTokenMessage));
    }

    [Test]
    public void EditPatient_SuperUser_ThrowsException()
    {
        var token = JwtService.GenerateJwt(_superUser);

        var patients = DbContext.Patients.ToList();

        async Task Action() => await _controller.EditPatient(token, patients[0].Id, UpdatedPersonPayload); 
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public void EditPatient_AdminUser_ThrowsException()
    {
        var token = JwtService.GenerateJwt(_adminUser);

        var patients = DbContext.Patients.ToList();

        async Task Action() => await _controller.EditPatient(token, patients[0].Id, UpdatedPersonPayload); 
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public async Task GetPatientById_WithLoggedInDoctor_SuccessfullyRetrievePatient()
    {
        var token = JwtService.GenerateJwt(_doctorUser);
        
        var response = await _controller.CreatePatient(token, PersonPayload);
        
        var patient = _controller.GetPatientById(response.Id, token);

        Assert.That(patient, Is.EqualTo(response));
    }

    [Test]
    public void GetPatientById_NotDoctorsPatient_ThrowException()
    {
        var token = JwtService.GenerateJwt(_doctorUser);

        var patients = DbContext.Patients.ToList();

        Task Action() { _controller.GetPatientById(patients[0].Id, token); return Task.CompletedTask; }
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You can only access patients registered to you"));
    }

    [Test]
    public void GetPatientById_WithNotLoggedInUser_ThrowsMessage()
    {
        var token = JwtService.GenerateJwt(_doctorUser);

        var patients = DbContext.Patients.ToList();

        var exception = Assert.ThrowsAsync<SecurityTokenMalformedException>(() => {
            _controller.GetPatientById(patients[0].Id, "Bob");
            return Task.CompletedTask;
        });

        Assert.That(exception.Message, Is.EqualTo(_expectedTokenMessage));
    }

    [Test]
    public void GetPatientById_SuperUser_ThrowsException()
    {
        var token = JwtService.GenerateJwt(_superUser);

        var patients = DbContext.Patients.ToList();

        Task Action() { _controller.GetPatientById(patients[0].Id, token); return Task.CompletedTask; }
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public void GetPatientById_AdminUser_ThrowsException()
    {
        var token = JwtService.GenerateJwt(_adminUser);

        var patients = DbContext.Patients.ToList();

        Task Action() { _controller.GetPatientById(patients[0].Id, token); return Task.CompletedTask; }
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public async Task DeletePatient_NotDoctorsPatient_ThrowException()
    {
        var token = JwtService.GenerateJwt(_doctorUser);

        var response = await _controller.CreatePatient(token, PersonPayload);
        var patients = DbContext.Patients.ToList();

        async Task Action() => await _controller.DeletePatient(token, patients[0].Id);
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"This user is not allowed to delete patient with id: {patients[0].Id}"));
    }

    [Test]
    public async Task DeletePatient_WrongPatientId_ThrowException()
    {
        var token = JwtService.GenerateJwt(_doctorUser);
        await _controller.CreatePatient(token, PersonPayload);

        async Task Action() => await _controller.DeletePatient(token, _fakePatientId);
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        Assert.That(exception.Message, Is.EqualTo($"Unable to locate patient with id: {_fakePatientId}"));
    }

    [Test]
    public void DeletePatient_WithNotLoggedInUser_ThrowsMessage()
    {
        var token = JwtService.GenerateJwt(_doctorUser);
        var patients = DbContext.Patients.ToList();
        
        var exception = Assert.ThrowsAsync<SecurityTokenMalformedException>(async () =>
        {
            await _controller.DeletePatient("Bob", patients[0].Id);
        });

        Assert.That(exception.Message, Is.EqualTo(_expectedTokenMessage));
    }

    [Test]
    public void DeletePatient_SuperUser_ThrowsException()
    {
        var token = JwtService.GenerateJwt(_superUser);
        var patients = DbContext.Patients.ToList();

        async Task Action() => await _controller.DeletePatient(token, patients[0].Id);  
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public void DeletePatient_AdminUser_ThrowsException()
    {
        var token = JwtService.GenerateJwt(_adminUser);
        var patients = DbContext.Patients.ToList();

        async Task Action() => await _controller.DeletePatient(token, patients[0].Id);
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public async Task GetPatients_WithLoggedInDoctor_SuccessfullFindPatient()
    {
        var token = JwtService.GenerateJwt(_doctorUser);
        var response = await _controller.CreatePatient(token, PersonPayload);

        var results = _controller.GetPatients(token, response.Email, response.FirstName + " " + response.LastName, response.Address);
        
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Email, Is.EqualTo(response.Email));
        Assert.That(results[0].FirstName, Is.EqualTo(response.FirstName));
        Assert.That(results[0].LastName, Is.EqualTo(response.LastName));
    }

    [Test]
    public async Task GetPatients_UsingOnlyToken_SuccessfullListPatients()
    {
        var token = JwtService.GenerateJwt(_doctorUser);

        await _controller.CreatePatient(token, PersonPayload);
        await _controller.CreatePatient(token, PersonPayload02);

        var results = _controller.GetPatients(token, null, null, null);
        for (int i = 0; i < results.Count; i++)
        {
            Console.WriteLine($"Name of doc patient {i + 1} = {results[i].LastName}");
            Assert.That(results[i].DoctorId, Is.EqualTo(_doctorUser.Id));
        }
    }

    [Test]
    public void GetPatients_WithNotLoggedInUser_ThrowsMessage()
    {
        var token = JwtService.GenerateJwt(_doctorUser);

        var patients = DbContext.Patients.ToList();

        var exception = Assert.ThrowsAsync<SecurityTokenMalformedException>(() => {
            _controller.GetPatientById(patients[0].Id, "Bob");
            return Task.CompletedTask;
        });

        Assert.That(exception.Message, Is.EqualTo(_expectedTokenMessage));
    }

    [Test]
    public async Task GetPatients_DoctorsPatientsOnly_SuccessfullCreation()
    {
        var token = JwtService.GenerateJwt(_doctorUser);

        var response = await _controller.CreatePatient(token, PersonPayload);

        var results = _controller.GetPatients(token, response.Email, $"{response.FirstName} {response.LastName}", response.Address);
        
        Assert.That(results[0].DoctorId, Is.EqualTo(_doctorUser.Id));
    }

    [Test]
    public void GetPatients_SuperUser_ThrowsException()
    {
        var token = JwtService.GenerateJwt(_superUser);

        Task Action() { _controller.GetPatients(token, null, null, null); return Task.CompletedTask; }
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    [Test]
    public void GetPatients_AdminUser_ThrowsException()
    {
        var token = JwtService.GenerateJwt(_adminUser);

        Task Action() { _controller.GetPatients(token, null, null, null); return Task.CompletedTask; }
        HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo($"You don't have the correct authorization"));
    }

    // Unable to test DeletePatient method, because:
    // ExecuteUpdate and ExecuteDelete are not supported by the in-memory database.
}
