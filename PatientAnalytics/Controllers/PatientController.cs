using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Services;
using Microsoft.AspNetCore.SignalR;
using PatientAnalytics.Hubs;

namespace PatientAnalytics.Controllers;

[ApiController]
[Authorize(Roles = "Doctor")]
[Route("/patients")]
public class PatientController
{
    private readonly JwtService _jwtService;
    private readonly Context _context;
    private readonly IHubContext<PatientHub> _hubContext;

    public PatientController(JwtService jwtService, Context context, IHubContext<PatientHub> hubContext)
    {
        _jwtService = jwtService;
        _context = context;
        _hubContext = hubContext;
    }
    
    [HttpGet("{patientId:int}", Name = "GetPatient")]
    public Patient GetPatientById([FromRoute] int patientId, [FromHeader] string authorization)
    {
        var user = _jwtService.GetUserWithJwt(authorization);

        if (user.Role != "Doctor")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You don't have the correct authorization");
        }

        var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);

        if (patient is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, $"Unable to find patient with id: {patientId}");
        }

        if (patient.DoctorId != user.Id)
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You can only access patients registered to you");
        }
        
        return patient;
    }

    [HttpGet(Name = "GetPatients")]
    public List<Patient> GetPatients(
        [FromHeader] string authorization, 
        [FromQuery] string? email, 
        [FromQuery] string? name,
        [FromQuery] string? address)
    {
        var user = _jwtService.GetUserWithJwt(authorization);

        if (user.Role != "Doctor")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You don't have the correct authorization");
        }

        var query = _context.Patients.Where(p => p.DoctorId == user.Id);
        
        if (email is not null)
        {
            query = query.Where(p => p.Email.ToLower().Contains(email.ToLower()));
        }
        
        if (name is not null)
        {
            query = query
                .Where(p => 
                    p.FirstName != null && p.FirstName.ToLower().Contains(name.ToLower()) || 
                    p.LastName != null && p.LastName.ToLower().Contains(name.ToLower()) || 
                    p.FirstName != null && p.LastName != null && (p.FirstName + " " + p.LastName).ToLower().Contains(name.ToLower()));
        }

        if (address is not null)
        {
            query = query.Where(p => p.Address != null && p.Address.ToLower().Contains(address.ToLower()));
        }

        return query.ToList();
    }

    [HttpPost(Name = "CreatePatient")]
    public async Task<Patient?> CreatePatient([FromHeader] string authorization, [FromBody] Person payload)
    {
        var user = _jwtService.GetUserWithJwt(authorization);

        if (user.Role != "Doctor")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You don't have the correct authorization");
        }

        if (_context.Patients.FirstOrDefault(p => p.Email == payload.Email) is not null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                $"Email address {payload.Email} already taken");
        }
        
        var patient = Patient.CreatePatient(user.Id, payload);
        
        _context.Patients.Add(patient);

        await _context.SaveChangesAsync();
        
        await _hubContext.Clients.All.SendAsync("ReceiveNewPatient", patient);

        return patient;
    }

    [HttpPut("{patientId:int}", Name = "EditPatient")]
    public async Task<Patient> EditPatient([FromHeader] string authorization, [FromRoute] int patientId, [FromBody] Person payload)
    {
        var user = _jwtService.GetUserWithJwt(authorization);

        if (user.Role != "Doctor")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You don't have the correct authorization");
        }

        var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);

        if (patient is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, $"Unable to locate patient with id: {patientId}");
        }

        if (patient.DoctorId != user.Id)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, $"This user is forbidden from editing patient with id: {patientId}");
        }

        var patientWithIdenticalEmail = _context.Patients.FirstOrDefault(p => p.Email == payload.Email);
        
        if (patientWithIdenticalEmail is not null && patientWithIdenticalEmail != patient)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, $"Email address {payload.Email} already taken");
        }

        patient.UpdatePatient(payload);

        _context.Patients.Update(patient);

        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ReceiveUpdatedPatient", patient);

        return patient;
    }

    [HttpDelete("{patientId:int}", Name = "DeletePatient")]
    public async Task<IActionResult> DeletePatient([FromHeader] string authorization, [FromRoute] int patientId)
    {
        var user = _jwtService.GetUserWithJwt(authorization);

        if (user.Role != "Doctor")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You don't have the correct authorization");
        }

        var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);
        
        if (patient is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, $"Unable to locate patient with id: {patientId}");
        }
        
        if (patient.DoctorId != user.Id)
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, $"This user is not allowed to delete patient with id: {patientId}");
        }

        // TODO: Delete Medical Records in Metrics API when Deleting Patient entirely
        await _context.Patients.Where(p => p.Id == patientId).ExecuteDeleteAsync();

        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("DeletedPatient", patient);

        return new NoContentResult();
    }
}