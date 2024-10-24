using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Services;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Controllers;

[ApiController]
[Authorize(Roles = $"{nameof(Role.Doctor)}")]
[Route("/api/patients")]
public class PatientController
{
    private readonly PatientService _patientService;
    private readonly ReportService _reportService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public PatientController(
        PatientService patientService,
        ReportService reportService,
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _patientService = patientService;
        _reportService = reportService;
        _localized = localized;
    }
    
    [HttpGet("{patientId:int}", Name = "GetPatient")]
    public async Task<Patient> GetPatientById([FromServices] IHttpContextAccessor httpContextAccessor, [FromRoute] int patientId)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return await _patientService.GetPatientById(authorization, patientId);
    }

    [HttpGet(Name = "GetPatients")]
    public List<Patient> GetPatients(
        [FromServices] IHttpContextAccessor httpContextAccessor, 
        [FromQuery] string? email, 
        [FromQuery] string? name,
        [FromQuery] string? address)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return _patientService.GetPatients(authorization, email, name, address);
    }

    [HttpPost(Name = "CreatePatient")]
    public async Task<Patient?> CreatePatient([FromServices] IHttpContextAccessor httpContextAccessor, [FromBody] PersonPayload payload)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return await _patientService.CreatePatient(authorization, payload);
    }

    [HttpPut("{patientId:int}", Name = "EditPatient")]
    public async Task<Patient> EditPatient([FromServices] IHttpContextAccessor httpContextAccessor, [FromRoute] int patientId, [FromBody] PersonPayload payload)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return await _patientService.EditPatient(authorization, patientId, payload);
    }

    [HttpDelete("{patientId:int}", Name = "DeletePatient")]
    public async Task<IActionResult> DeletePatient([FromServices] IHttpContextAccessor httpContextAccessor, [FromRoute] int patientId)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return await _patientService.DeletePatient(authorization, patientId);
    }
    
    [HttpGet("{patientId:int}/report", Name = "GetPatientReport")]
    public async Task<IResult> GetPatientReportById([FromServices] IHttpContextAccessor httpContextAccessor, [FromRoute] int patientId)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        var patient = await _patientService.GetPatientById(authorization, patientId);
        var reportResponse = _reportService.GenerateReportForPatient(patient);

        return Results.File(reportResponse.fileStream, "application/pdf", reportResponse.fileName);
    }

    private void ValidateAuthorization(IHttpContextAccessor httpContextAccessor, out string verifiedAuthorization)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() 
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["HeaderError_Authorization"]);

        verifiedAuthorization = authorization;
    }
}