using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PatientAnalytics.Hubs;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Services;
using PatientAnalytics.Utils;
using PatientAnalytics.Utils.Localization;
using SignalR_UnitTestingSupportCommon.IHubContextSupport;

namespace PatientAnalytics.Tests;
public abstract class BaseTest
{
    protected static readonly Context DbContext = GetInMemoryTestDatabase();

    private static readonly Dictionary<string, string> TestConfiguration = new()
    {
        { "Jwt:Key", "tp6osj2CoTOIz8pvIUV0Vg1T0vlkZFD14nEtLYZ75FFfaQWQ7qMcU6byLyIkqArgVQT3P1YGvwHumjqZJtOtW3VMG2OLR9tpHKcC" },
        { "Jwt:Issuer", "http://localhost:8080/" },
        { "Jwt:Audience", "http://localhost:8080/" },
        { "Jwt:ExpirationTime", "1" },
        { "Auth:Salt", "passwordSalt" }
    };

    protected static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(TestConfiguration)
        .Build();

    private static readonly IOptions<LocalizationOptions> LocalizationOptionsDefined = Options.Create(new LocalizationOptions());
    private static readonly ResourceManagerStringLocalizerFactory LocalizerFactory = 
        new(LocalizationOptionsDefined, NullLoggerFactory.Instance);

    protected static readonly IStringLocalizer<ApiResponseLocalized> Localized =
        new StringLocalizer<ApiResponseLocalized>(LocalizerFactory);
    
    protected static readonly IHubContext<PatientHub> HubContext = GetHubContext();

    protected static readonly JwtService JwtService = new(DbContext, Configuration, Localized);

    protected static readonly AuthService AuthService = new(DbContext, JwtService, Configuration, Localized);

    protected const string UserPassword = "axyn234x2a!";
    private static readonly string PasswordHash = Password.HashPassword(UserPassword, Configuration);

    private static Context GetInMemoryTestDatabase()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase("PatientAnalyticsTestDatabase")
            .Options;

        return new Context(options);
    }

    private static IHubContext<PatientHub> GetHubContext()
    {
        var iHubContextSupport = new UnitTestingSupportForIHubContext<PatientHub>();
        return iHubContextSupport.IHubContextMock.Object;
    }

    protected static User CreateSuperUserForTest()
    {
        // Define and return the super user for this test class
        return User.CreateUser(
            PasswordHash,
            new RegistrationPayload
            {
                DateOfBirth = DateTime.UtcNow,
                Gender = "Male",
                Email = "email@email.com",
                Username = "testUserSuper",
                Password = UserPassword,
                Address = "address",
                FirstName = "firstname1",
                LastName = "lastname1"
            },
            Role.SuperAdmin
        );
    }

    protected static IHttpContextAccessor CreateHttpContextAccessor(string authorization)
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers["Authorization"] = $"Bearer {authorization}";
        
        mockHttpContextAccessor.Setup(req => req.HttpContext).Returns(httpContext);

        return mockHttpContextAccessor.Object;
    }

    protected static User CreateAdminUserForTest()
    {
        // Define and return the admin user for this test class
        return User.CreateUser(
            PasswordHash,
            new RegistrationPayload
            {
                DateOfBirth = DateTime.UtcNow,
                Gender = "Female",
                Email = "email2@email.com",
                Username = "testUserAdmin",
                Password = UserPassword,
                Address = "address",
                FirstName ="firstname2",
                LastName = "lastname2"
            },
            Role.Admin
        );
    }

    protected static User CreateDoctorUserForTest()
    {
        // Define and return the doctor user for this test class
        return User.CreateUser(
            PasswordHash,
            new RegistrationPayload
            {
                DateOfBirth = DateTime.UtcNow,
                Gender = "Male",
                Email = "email3@email.com",
                Username = "testUserDoctor",
                Password = UserPassword,
                Address = "address",
                FirstName = "firstname3",
                LastName = "lastname3"
            },
            Role.Doctor
        );
    }

    protected static void AddSaveChanges(User newUser)
    {
        DbContext.Users.Add(newUser);
        DbContext.SaveChanges();
    }

    protected static void ClearUsers()
    {
        var allUsers = DbContext.Users.ToList();
        foreach (var user in allUsers)
        {
            DbContext.Users.Remove(user);
        }
        DbContext.SaveChanges();
    }
}
