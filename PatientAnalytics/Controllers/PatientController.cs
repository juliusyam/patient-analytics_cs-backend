using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PatientAnalytics.Hubs;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Services;

namespace PatientAnalytics.Controllers;

[ApiController]
[Authorize(Roles = "Doctor")]
[Route("/patients")]
public class PatientController
{
    private readonly JwtService _jwtService;
    private readonly PatientService _patientService;
    private readonly Context _context;
    private readonly IHubContext<PatientHub> _hubContext;

    public PatientController(JwtService jwtService, PatientService patientService, Context context, IHubContext<PatientHub> hubContext)
    {
        _jwtService = jwtService;
        _patientService = patientService;
        _context = context;
        _hubContext = hubContext;
    }
    
    [HttpGet("{patientId:int}", Name = "GetPatient")]
    public Patient GetPatientById([FromServices] IHttpContextAccessor httpContextAccessor, [FromRoute] int patientId)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() ?? "";

        return _patientService.GetPatientById(authorization, patientId);
    }

    [HttpGet(Name = "GetPatients")]
    public List<Patient> GetPatients(
        [FromServices] IHttpContextAccessor httpContextAccessor, 
        [FromQuery] string? email, 
        [FromQuery] string? name,
        [FromQuery] string? address)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() ?? "";

        return _patientService.GetPatients(authorization, email, name, address);
    }

    [HttpPost(Name = "CreatePatient")]
    public async Task<Patient?> CreatePatient([FromServices] IHttpContextAccessor httpContextAccessor, [FromBody] Person payload)
    {
        var authorization = httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
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
    public async Task<Patient> EditPatient([FromServices] IHttpContextAccessor httpContextAccessor, [FromRoute] int patientId, [FromBody] Person payload)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() ?? "";

        return await _patientService.EditPatient(authorization, patientId, payload);
    }

    [HttpDelete("{patientId:int}", Name = "DeletePatient")]
    public async Task<IActionResult> DeletePatient([FromRoute] int patientId, [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var authorization = httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
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