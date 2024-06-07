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
    private static readonly PatientService PatientService = new (DbContext, JwtService, HubContext, Localized);
    protected static readonly PatientController PatientController = new (PatientService, Localized);

    private User _superUser;
    private User _adminUser;
    protected User DoctorUser;
    private Patient _patientZero;
    
    private string _superAdminUserJwt;
    private string _adminUserJwt;
    private string _doctorUserJwt;

    protected IHttpContextAccessor SuperAdminHttpContextAccessor;
    protected IHttpContextAccessor AdminHttpContextAccessor;
    protected IHttpContextAccessor DoctorHttpContextAccessor;

    [SetUp]
    public void SetUp()
    {
        _superUser = CreateSuperUserForTest();
        AddSaveChanges(_superUser);

        _adminUser = CreateAdminUserForTest();
        AddSaveChanges(_adminUser);

        DoctorUser = CreateDoctorUserForTest();
        AddSaveChanges(DoctorUser);

        _patientZero = CreatePatientForTest();
        AddPatientSaveChanges(_patientZero);
        
        _superAdminUserJwt = JwtService.GenerateJwt(_superUser);
        _adminUserJwt = JwtService.GenerateJwt(_adminUser);
        _doctorUserJwt = JwtService.GenerateJwt(DoctorUser);

        SuperAdminHttpContextAccessor = CreateHttpContextAccessor(_superAdminUserJwt);
        AdminHttpContextAccessor = CreateHttpContextAccessor(_adminUserJwt);
        DoctorHttpContextAccessor = CreateHttpContextAccessor(_doctorUserJwt);
    }
    
    [TearDown]
    public void TearDown()
    {
        ClearUsers();
        ClearPatients();
    }

    protected const string ExpectedTokenMessage = "IDX12709: " +
        "CanReadToken() returned false. JWT is not well formed." +
        "\nThe token needs to be in JWS or JWE Compact Serialization Format. " +
        "(JWS): 'EncodedHeader.EndcodedPayload.EncodedSignature'. " +
        "(JWE): 'EncodedProtectedHeader.EncodedEncryptedKey.EncodedInitializationVector.EncodedCiphertext.EncodedAuthenticationTag'.";
    
    protected const int FakePatientId = 999;

    protected static readonly Person PersonPayload = new Person(
        DateTime.Now,
        "gender",
        "email2",
        "address02",
        "firstnamePatientTest02",
        "lastnamePatientTest02",
        DateTime.Now,
        DateTime.Now
     );

    protected static readonly Person UpdatedPersonPayload = new Person(
        DateTime.Now,
        "gender",
        "email2",
        "address03",
        "firstnamePatientTest02",
        "lastnamePatientTest02",
        DateTime.Now,
        DateTime.Now
     );

    protected static readonly Person UpdatedPersonPayload02 = new Person(
        DateTime.Now,
        "gender",
        "email01",
        "address03",
        "firstnamePatientTest02",
        "lastnamePatientTest02",
        DateTime.Now,
        DateTime.Now
     );

    protected static readonly Person PersonPayload02 = new Person(
        DateTime.Now,
        "gender",
        "email04",
        "address02",
        "firstnamePatientTest04",
        "lastnamePatientTest04",
        DateTime.Now,
        DateTime.Now
     );

    private static Patient CreatePatientForTest()
    {
        // Define and return the patient for this test class
        return Patient.CreatePatient(
            666,
            new Person(
                DateTime.Now,
                "gender01",
                "email01",
                "address01",
                "firstnamePatientTest01",
                "lastnamePatientTest01",
                DateTime.Now,
                DateTime.Now
            )
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
