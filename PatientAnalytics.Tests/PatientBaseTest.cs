using PatientAnalytics.Controllers;
using PatientAnalytics.Models;
using System;
using System.Linq;

namespace PatientAnalytics.Tests;
public abstract class PatientBaseTest : BaseTest
{
    protected readonly PatientController _controller = new(JwtService, DbContext);

    protected User _superUser;
    protected User _adminUser;
    protected User _doctorUser;
    protected Patient _patientZero;
    protected string _expectedTokenMessage = "IDX12709: " +
        "CanReadToken() returned false. JWT is not well formed." +
        "\nThe token needs to be in JWS or JWE Compact Serialization Format. " +
        "(JWS): 'EncodedHeader.EndcodedPayload.EncodedSignature'. " +
        "(JWE): 'EncodedProtectedHeader.EncodedEncryptedKey.EncodedInitializationVector.EncodedCiphertext.EncodedAuthenticationTag'.";
    protected int _fakePatientId = 999;

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

    protected static Patient CreateNonUserPatientForTest()
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

    protected static void AddPatientSaveChanges(Patient newUser)
    {
        DbContext.Patients.Add(newUser);
        DbContext.SaveChanges();
    }

    protected static void ClearPatients()
    {
        var allPatients = DbContext.Patients.ToList();
        foreach (var patients in allPatients)
        {
            DbContext.Patients.Remove(patients);
        }
        DbContext.SaveChanges();
    }
}
