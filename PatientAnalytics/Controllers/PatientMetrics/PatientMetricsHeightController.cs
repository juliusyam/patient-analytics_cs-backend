using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models.PatientMetrics;
using PatientAnalytics.Services.PatientMetrics;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Controllers.PatientMetrics;

[ApiController]
[Authorize(Roles = "Doctor")]
[Route("/api")]
public class PatientMetricsHeightController : Controller
{
    private readonly PatientMetricsHeightService _heightService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public PatientMetricsHeightController(
        PatientMetricsHeightService heightService, 
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _heightService = heightService;
        _localized = localized;
    }
    
    [HttpPost("patients/{patientId:int}/heights", Name = "CreateHeightEntry")]
    public async Task<PatientHeight> CreateEntry(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int patientId,
        [FromBody] PatientHeightPayload payload)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _heightService.CreateEntry(token, patientId, payload);
    }
    
    [HttpGet("patients/{patientId:int}/heights", Name = "GetPatientHeights")]
    public async Task<List<PatientHeight>> GetPatientHeights(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int patientId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _heightService.GetPatientHeights(token, patientId);
    }

    [HttpGet("heights/{heightRecordId:int}", Name = "GetHeightEntryById")]
    public PatientHeight GetEntryById(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int heightRecordId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return _heightService.GetEntryById(token, heightRecordId);
    }

    [HttpDelete("heights/{heightRecordId:int}", Name = "DeleteHeightEntryById")]
    public Task<IActionResult> DeleteEntryById(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int heightRecordId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return _heightService.DeleteEntryById(token, heightRecordId);
    }
    
    private void ValidateAuthorization(IHttpContextAccessor httpContextAccessor, out string verifiedAuthorization)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() 
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["HeaderError_Authorization"]);

        verifiedAuthorization = authorization;
    }
}