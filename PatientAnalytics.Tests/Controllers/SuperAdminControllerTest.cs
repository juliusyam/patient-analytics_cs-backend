using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using PatientAnalytics.Controllers;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Services;

namespace PatientAnalytics.Tests.Controllers;

[TestFixture]
public class SuperAdminControllerTest : BaseTest
{
    private static readonly RegistrationService RegistrationService = 
        new(DbContext, Configuration, JwtService, HubContext, Localized);

    private static readonly SuperAdminController SuperAdminController =
        new(RegistrationService, Localized);
    
    private static readonly RegistrationPayload Payload = new()
    {
        DateOfBirth = DateTime.Now,
        Gender = "Male",
        Email = "email1@email.com",
        Username = "testUserAdmin1",
        Password = UserPassword,
        Address = "address",
        FirstName = "First-Name",
        LastName = "Last-Name"
    };

    private User _superUser;
    private User _adminUser;
    private User _doctorUser;

    private string _superUserJwt;
    private string _adminUserJwt;
    private string _doctorUserJwt;

    [SetUp]
    public void SetUp()
    {
        _superUser = CreateSuperUserForTest();
        AddSaveChanges(_superUser);

        _adminUser = CreateAdminUserForTest();
        AddSaveChanges(_adminUser);

        _doctorUser = CreateDoctorUserForTest();
        AddSaveChanges(_doctorUser);

        _superUserJwt = JwtService.GenerateJwt(_superUser);
        _adminUserJwt = JwtService.GenerateJwt(_adminUser);
        _doctorUserJwt = JwtService.GenerateJwt(_doctorUser);
    }
    
    [TearDown]
    public void TearDown() => ClearUsers();
    
    // Test that a SuperAdmin can create other super admins
    [Test]
    public async Task RegisterSuperAdmin_SuperAdminCreate_AdminCreated()
    {
        var currentUserCount = DbContext.Users.Count();

        var response = await SuperAdminController.RegisterSuperAdmin(CreateHttpContextAccessor(_superUserJwt), Payload);
        
        Assert.That(DbContext.Users.Count(), Is.EqualTo(currentUserCount + 1));
        Assert.That(response.User.Role, Is.EqualTo("SuperAdmin"));
        Assert.That(response.Token, Is.Not.Null);
    }
    
    // Test that Admins can’t create super admins
    [Test]
    public void RegisterSuperAdmin_AdminCreate_ThrowsUnauthorizedException()
    {
        async Task Action() => await SuperAdminController.RegisterSuperAdmin(CreateHttpContextAccessor(_adminUserJwt), Payload);
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);
        
        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
    }

    // Test that doctors can’t create super admins
    [Test]
    public void RegisterSuperAdmin_DoctorCreate_ThrowsUnauthorizedException()
    {
        async Task Action() => await SuperAdminController.RegisterSuperAdmin(CreateHttpContextAccessor(_doctorUserJwt), Payload);
        var exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
    }
}