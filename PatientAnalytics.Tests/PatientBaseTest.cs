using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using PatientAnalytics.Controllers;
using PatientAnalytics.Models;
using PatientAnalytics.Services;

namespace PatientAnalytics.Tests;
public abstract class PatientBaseTest : BaseTest
{
    protected static readonly PatientService PatientService = new(DbContext, JwtService, HubContext, Localized);
    protected static readonly ReportService ReportService = new(Localized);
    protected static readonly PatientController PatientController = new(PatientService, ReportService, Localized);
        
    protected User _superUser;
    private User _adminUser;
    protected User DoctorUser;
    protected User Doctor02User;
    protected Patient _patientZero;
    protected Patient _patientOne;
    protected Patient _patient;

    private string _superAdminUserJwt;
    private string _adminUserJwt;
    private string _doctorUserJwt;
    private string _doctor02UserJwt;

    protected IHttpContextAccessor SuperAdminHttpContextAccessor;
    protected IHttpContextAccessor AdminHttpContextAccessor;
    protected IHttpContextAccessor DoctorHttpContextAccessor;
    protected IHttpContextAccessor Doctor02HttpContextAccessor;

    [SetUp]
    public void SetUp()
    {
        _superUser = CreateSuperUserForTest();
        AddSaveChanges(_superUser);

        _adminUser = CreateAdminUserForTest();
        AddSaveChanges(_adminUser);

        DoctorUser = CreateDoctorUserForTest();
        AddSaveChanges(DoctorUser);

        Doctor02User = CreateDoctorUserForTest();
        AddSaveChanges(Doctor02User);

        _patientZero = CreateTestPatient();
        AddPatientSaveChanges(_patientZero);
        _patientOne = CreateTestPatient(DoctorUser.Id);
        AddPatientSaveChanges(_patientOne);

        _superAdminUserJwt = JwtService.GenerateJwt(_superUser);
        _adminUserJwt = JwtService.GenerateJwt(_adminUser);
        _doctorUserJwt = JwtService.GenerateJwt(DoctorUser);
        _doctor02UserJwt = JwtService.GenerateJwt(Doctor02User);

        SuperAdminHttpContextAccessor = CreateHttpContextAccessor(_superAdminUserJwt);
        AdminHttpContextAccessor = CreateHttpContextAccessor(_adminUserJwt);
        DoctorHttpContextAccessor = CreateHttpContextAccessor(_doctorUserJwt);
        Doctor02HttpContextAccessor = CreateHttpContextAccessor(_doctor02UserJwt);
    }    

    [TearDown]
    public void TearDown()
    {
        ClearUsers();
        ClearPatients();
    }

    protected const int FakePatientId = 999;

    protected static readonly PersonPayload PersonPayload = new()
    {
        DateOfBirth = DateTime.UtcNow,
        Gender = "gender",
        Email = "email2",
        Address = "address02",
        FirstName = "firstnamePatientTest02",
        LastName = "lastnamePatientTest02"
    };

    protected static readonly PersonPayload UpdatedPersonPayload = new()
    {
        DateOfBirth = DateTime.UtcNow,
        Gender = "gender",
        Email = "email2",
        Address = "address03",
        FirstName = "firstnamePatientTest02",
        LastName = "lastnamePatientTest02"
    };

    protected static readonly PersonPayload UpdatedPersonPayload02 = new()
    {
        DateOfBirth = DateTime.UtcNow,
        Gender = "gender",
        Email = "email01",
        Address = "address03",
        FirstName = "firstnamePatientTest02",
        LastName = "lastnamePatientTest02"
    };

    protected static readonly PersonPayload PersonPayload02 = new()
    {
        DateOfBirth = DateTime.UtcNow,
        Gender = "gender",
        Email = "email04",
        Address = "address02",
        FirstName = "firstnamePatientTest04",
        LastName = "lastnamePatientTest04"
    };

    private static Patient CreateTestPatient(int doctorId = 666)
    {
        // Define and return the patient for this test class
        return Patient.CreatePatient(
            doctorId,
            new PersonPayload()
            {
                DateOfBirth = DateTime.UtcNow,
                Gender = "gender01",
                Email = "email01",
                Address = "address01",
                FirstName = "firstnamePatientTest01",
                LastName = "lastnamePatientTest01"
            }
        );
    }

    private static void AddPatientSaveChanges(Patient newUser)
    {
        DbContext.Patients.Add(newUser);
        DbContext.SaveChanges();
    }

    private static void ClearPatients()
    {
        var allPatients = DbContext.Patients.ToList();
        foreach (var patients in allPatients)
        {
            DbContext.Patients.Remove(patients);
        }
        DbContext.SaveChanges();
    }
}
