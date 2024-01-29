using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using PatientAnalytics.Controllers;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PatientAnalytics.Tests.Controllers
{
    [TestFixture]

    public class RegistrationControllerTest : BaseTest
    {
        private readonly RegistrationController _controller = new(AuthService, JwtService);
        private readonly SuperAdminController _controllerSuperAdmin = new(AuthService, JwtService);

        private static readonly RegistrationPayload Payload = new RegistrationPayload(
        DateTime.Now,
        "gender",
        "email1",
        "testUserAdmin1",
        UserPassword,
        "address",
        "firstnametest1",
        "lastnametest1"
    );

        private User _superUser;
        private User _adminUser;
        private User _doctorUser;

        private string _superUserJWT;
        private string _adminUserJWT;
        private string _doctorUserJWT;

        [SetUp]
        public void SetUp()
        {
            _superUser = CreateSuperUserForTest();
            AddSaveChanges(_superUser);

            _adminUser = CreateAdminUserForTest();
            AddSaveChanges(_adminUser);

            _doctorUser = CreateDoctorUserForTest();
            AddSaveChanges(_doctorUser);

            _superUserJWT = JwtService.GenerateJwt(_superUser);
            _adminUserJWT = JwtService.GenerateJwt(_adminUser);
            _doctorUserJWT = JwtService.GenerateJwt(_doctorUser);
        }

        [TearDown]
        public void TearDown()
        {
            ClearUsers();
        }

        // Test that a SuperAdmin can create other super admins
        [Test]
        public async Task RegisterSuperAdmin_SuperAdminCreate_AdminCreated()
        {
            var currentUserCount = DbContext.Users.Count();

            var response = await _controllerSuperAdmin.RegisterSuperAdmin(_superUserJWT, Payload);
            
            Assert.That(DbContext.Users.Count(), Is.EqualTo(currentUserCount + 1));
            Assert.That(response.User.Role, Is.EqualTo("SuperAdmin"));
            Assert.That(response.Token, Is.Not.Null);
        }

        // Test that SuperAdmins can create admins
        [Test]
        public async Task RegisterAdmin_SuperAdminCreate_AdminCreated()
        {
            var currentUserCount = DbContext.Users.Count();

            var response = await _controller.RegisterAdmin(_superUserJWT, Payload);

            Assert.That(DbContext.Users.Count(), Is.EqualTo(currentUserCount + 1));
            Assert.That(response.User.Role, Is.EqualTo("Admin"));
            Assert.That(response.Token, Is.Not.Null);
        }

        // Test that SuperAdmins can create doctors
        [Test]
        public async Task RegisterDoctor_SuperAdminCreate_DoctorCreated()
        {
            var currentUserCount = DbContext.Users.Count();
            
            var response = await _controller.RegisterDoctor(_superUserJWT, Payload);

            Assert.That(DbContext.Users.Count(), Is.EqualTo(currentUserCount + 1));
            Assert.That(response.User.Role, Is.EqualTo("Doctor"));
            Assert.That(response.Token, Is.Not.Null);
        }

        // Test that Admins can create other admins
        [Test]
        public async Task RegisterAdmin_AdminCreate_AdminCreated()
        {
            var currentUserCount = DbContext.Users.Count();
            
            var response = await _controller.RegisterAdmin(_adminUserJWT, Payload);

            Assert.That(DbContext.Users.Count(), Is.EqualTo(currentUserCount + 1));
            Assert.That(response.User.Role, Is.EqualTo("Admin"));
            Assert.That(response.Token, Is.Not.Null);
        }

        // Test that Admins can create doctors
        [Test]
        public async Task RegisterDoctor_AdminCreate_DoctorCreated()
        {
            var currentUserCount = DbContext.Users.Count();
            
            var response = await _controller.RegisterDoctor(_adminUserJWT, Payload);

            Assert.That(DbContext.Users.Count(), Is.EqualTo(currentUserCount + 1));
            Assert.That(response.User.Role, Is.EqualTo("Doctor"));
            Assert.That(response.Token, Is.Not.Null);
        }

        // Test that Admins can’t create super admins
        [Test]
        public void RegisterSuperAdmin_AdminCreate_ThrowsUnauthorizedException()
        {
            var currentUserCount = DbContext.Users.Count();

            async Task Action() => await _controllerSuperAdmin.RegisterSuperAdmin(_adminUserJWT, Payload);
            HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

            Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
            Assert.That(exception.Message, Is.EqualTo("You don't have the correct Super Admin authorization"));
        }

        // Test that doctors can’t create super admins
        [Test]
        public void RegisterSuperAdmin_DoctorCreate_ThrowsUnauthorizedException()
        {
            async Task Action() => await _controllerSuperAdmin.RegisterSuperAdmin(_doctorUserJWT, Payload);
            HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

            Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
            Assert.That(exception.Message, Is.EqualTo("You don't have the correct Super Admin authorization"));
        }

        // Test that doctors can’t create admins
        [Test]
        public void RegisterAdmin_DoctorCreate_ThrowsUnauthorizedException()
        {
            async Task Action() => await _controller.RegisterAdmin(_doctorUserJWT, Payload);
            HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

            Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
            Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
        }

        // Test that doctors can’t create doctors
        [Test]
        public void RegisterDoctor_DoctorCreate_ThrowsUnauthorizedException()
        {
            async Task Action() => await _controller.RegisterDoctor(_doctorUserJWT, Payload);
            HttpStatusCodeException exception = Assert.ThrowsAsync<HttpStatusCodeException>(Action);

            Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
            Assert.That(exception.Message, Is.EqualTo("You don't have the correct authorization"));
        }
    }
}
