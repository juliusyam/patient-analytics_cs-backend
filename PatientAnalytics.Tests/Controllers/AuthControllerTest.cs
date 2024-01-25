using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using PatientAnalytics.Controllers;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;

namespace PatientAnalytics.Tests.Controllers;

[TestFixture]
public class AuthControllerTest : BaseTest
{
    private readonly AuthController _controller = new (AuthService);

    private User _superUser; 
    private User _adminUser;
    private User _doctorUser;

    [SetUp]
    public void SetUp()
    {
        _superUser = CreateSuperUserForTest();
        AddSaveChanges(_superUser);

        _adminUser = CreateAdminUserForTest();
        AddSaveChanges(_adminUser);

        _doctorUser = CreateDoctorUserForTest();
        AddSaveChanges(_doctorUser);
    }

    [TearDown]
    public void TearDown()
    {
        RemoveSaveChanges(_superUser);
        RemoveSaveChanges(_adminUser);
        RemoveSaveChanges(_doctorUser);
    }

    [Test]
    public void Login_ValidSuperUserCredentials_SuccessfulLogin()
    {
        var payload = new LoginPayload(_superUser.Username, UserPassword);
        var response = _controller.Login(payload);

        Assert.That(_superUser, Is.EqualTo(response.User));
        Assert.That(response.Token, Is.Not.Null);
    }

    [Test]
    public void Login_ValidAdminUserCredentials_SuccessfulLogin()
    {
        var payload = new LoginPayload(_adminUser.Username, UserPassword);
        var response = _controller.Login(payload);
        
        Assert.That(_adminUser, Is.EqualTo(response.User));
        Assert.That(response.Token, Is.Not.Null);
    }

    [Test]
    public void Login_ValidDoctorUserCredentials_SuccessfulLogin()
    {
        var payload = new LoginPayload(_doctorUser.Username, UserPassword);
        var response = _controller.Login(payload);

        Assert.That(_doctorUser, Is.EqualTo(response.User));
        Assert.That(response.Token, Is.Not.Null);
    }

    [Test]
    public void Login_InvalidSuperUserPassword_ThrowsUnauthorizedException()
    {
        var payload = new LoginPayload(_superUser.Username, "incorrect password");

        HttpStatusCodeException exception = Assert.Throws<HttpStatusCodeException>(
        () => _controller.Login(payload)
        );

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(exception.Message, Is.EqualTo("Wrong password"));
    }

    [Test]
    public void Login_InvalidSuperUserUsername_ThrowsNotFoundException()
    {
        var payload = new LoginPayload("Bob", UserPassword);

        HttpStatusCodeException exception = Assert.Throws<HttpStatusCodeException>(
        () => _controller.Login(payload)
        );

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        Assert.That(exception.Message, Is.EqualTo("User not found"));
    }
}