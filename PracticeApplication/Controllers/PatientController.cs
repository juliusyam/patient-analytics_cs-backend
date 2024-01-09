using Microsoft.AspNetCore.Mvc;
using PracticeApplication.Middleware;
using PracticeApplication.Models;

namespace PracticeApplication.Controllers;

[ApiController]
[Route("/patients")]
public class PatientController
{
    [HttpGet("{patientId}", Name = "GetPatient")]
    public Patient? GetPatientById([FromServices] Context context, [FromRoute] int patientId)
    {
        var patient = context.Patients.FirstOrDefault(p => p.Id == patientId);

        if (patient is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, $"Unable to find patient with id: {patientId}");
        }
        
        return patient;
    }

    [HttpPost(Name = "CreatePatient")]
    public async Task<Patient?> CreatePatient([FromServices] Context context, [FromBody] Person.CreatePayload payload)
    {
        // TODO: To be replaced with Fetch User from token
        var doctorId = 1;
        
        var user = context.Users.FirstOrDefault(u => u.Id == doctorId);

        if (user is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, $"Unable to find doctor with id: {doctorId}");
        }
        
        var patientWithIdenticalEmail = context.Patients.FirstOrDefault(u => u.Email == payload.Email);

        if (patientWithIdenticalEmail is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, $"Email address {payload.Email} already taken");
        }
        
        var patient = Patient.CreatePatient(doctorId, payload);
        context.Patients.Add(patient);

        await context.SaveChangesAsync();

        return patient;
    }
}