using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.PatientMetrics;
using PatientAnalytics.Services.PatientMetrics;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Controllers.PatientMetrics;

[ApiController]
[Authorize(Roles = $"{nameof(Role.Doctor)}")]
[Route("/api")]
public class PatientMetricsBloodPressureController : Controller
{
    private readonly PatientMetricsBloodPressureService _bloodPressureService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public PatientMetricsBloodPressureController(
        PatientMetricsBloodPressureService bloodPressureService,
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _bloodPressureService = bloodPressureService;
        _localized = localized;
    }

    [HttpPost("patients/{patientId:int}/blood-pressures", Name = "CreateBloodPressureEntry")]
    public async Task<PatientBloodPressure> CreateEntry(
        [FromServices] IHttpContextAccessor httpContextAccessor, 
        [FromRoute] int patientId,
        [FromBody] PatientBloodPressurePayload payload)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _bloodPressureService.CreateEntry(token, patientId, payload);
    }

    [HttpGet("patients/{patientId:int}/blood-pressures", Name = "GetPatientBloodPressures")]
    public async Task<List<PatientBloodPressure>> GetPatientBloodPressures(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int patientId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _bloodPressureService.GetPatientBloodPressures(token, patientId);
    }

    [HttpGet("blood-pressures/{bloodPressureRecordId:int}", Name = "GetBloodPressureEntryById")]
    public PatientBloodPressure GetEntryById(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int bloodPressureRecordId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return _bloodPressureService.GetEntryById(token, bloodPressureRecordId);
    }
    
    [HttpDelete("blood-pressures/{bloodPressureRecordId:int}", Name = "DeleteBloodPressureEntryById")]
    public async Task<IActionResult> DeleteEntryById(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int bloodPressureRecordId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _bloodPressureService.DeleteEntryById(token, bloodPressureRecordId);
    }
    
    private void ValidateAuthorization(IHttpContextAccessor httpContextAccessor, out string verifiedAuthorization)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() 
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["HeaderError_Authorization"]);

        verifiedAuthorization = authorization;
    }
}