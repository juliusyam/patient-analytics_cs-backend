using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Services;
using PatientAnalytics.Utils;

namespace PatientAnalytics.Tests;
public abstract class BaseTest
{
    protected static readonly Context DbContext = GetInMemoryTestDatabase();

    private static readonly Dictionary<string, string> TestConfiguration = new()
    {
        { "Jwt:Key", "tp6osj2CoTOIz8pvIUV0Vg1T0vlkZFD14nEtLYZ75FFfaQWQ7qMcU6byLyIkqArgVQT3P1YGvwHumjqZJtOtW3VMG2OLR9tpHKcC" },
        {"Auth:Salt", "passwordSalt"}
    };

    protected static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(TestConfiguration)
        .Build();

    protected static readonly JwtService JwtService = new JwtService(DbContext, Configuration);

    protected static readonly UserService UserService = new UserService(DbContext, Configuration);

    protected static readonly AuthService AuthService = new AuthService(DbContext, JwtService, UserService, Configuration);

    protected const string UserPassword = "axyn234x2a!";
    protected static readonly string PasswordHash = Password.HashPassword(UserPassword, Configuration);

    private static Context GetInMemoryTestDatabase()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase("PatientAnalyticsTestDatabase")
            .Options;

        return new Context(options);
    }

    protected static User CreateSuperUserForTest()
    {
        // Define and return the super user for this test class
        return User.CreateUser(
            PasswordHash,
            new RegistrationPayload(
                DateTime.Now,
                "gender",
                "email",
                "testUserSuper",
                UserPassword,
                "address",
                "firstname1",
                "lastname1"
            ),
            "SuperAdmin"
        );
    }

    protected static User CreateAdminUserForTest()
    {
        // Define and return the admin user for this test class
        return User.CreateUser(
            PasswordHash,
            new RegistrationPayload(
                DateTime.Now,
                "gender",
                "email",
                "testUserAdmin",
                UserPassword,
                "address",
                "firstname2",
                "lastname2"
            ),
            "Admin"
        );
    }

    protected static User CreateDoctorUserForTest()
    {
        // Define and return the doctor user for this test class
        return User.CreateUser(
            PasswordHash,
            new RegistrationPayload(
                DateTime.Now,
                "gender",
                "email",
                "testUserDoctor",
                UserPassword,
                "address",
                "firstname3",
                "lastname3"
            ),
            "Doctor"
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
