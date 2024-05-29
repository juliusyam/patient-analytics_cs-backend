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
public class PatientMetricsTemperatureController : Controller
{
    private readonly PatientMetricsTemperatureService _temperatureService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public PatientMetricsTemperatureController(PatientMetricsTemperatureService temperatureService, IStringLocalizer<ApiResponseLocalized> localized)
    {
        _temperatureService = temperatureService;
        _localized = localized;
    }

    [HttpPost("patients/{patientId:int}/temperatures", Name = "CreateTemperatureEntry")]
    public async Task<PatientTemperature> CreateEntry(
        [FromServices] IHttpContextAccessor httpContextAccessor, 
        [FromRoute] int patientId,
        [FromBody] PatientTemperaturePayload payload)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _temperatureService.CreateEntry(token, patientId, payload);
    }

    [HttpGet("patients/{patientId:int}/temperatures", Name = "GetPatientTemperatures")]
    public async Task<List<PatientTemperature>> GetPatientTemperatures(
        [FromServices] IHttpContextAccessor httpContextAccessor, 
        [FromRoute] int patientId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _temperatureService.GetPatientTemperatures(token, patientId);
    }
    
    [HttpGet("temperatures/{temperatureRecordId:int}", Name = "GetTemperatureEntryById")]
    public PatientTemperature GetEntryById(
        [FromServices] IHttpContextAccessor httpContextAccessor, 
        [FromRoute] int temperatureRecordId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return _temperatureService.GetEntryById(token, temperatureRecordId);
    }
    
    [HttpDelete("temperatures/{temperatureRecordId:int}", Name = "DeleteTemperatureEntryById")]
    public async Task<IActionResult> DeleteEntryById(
        [FromServices] IHttpContextAccessor httpContextAccessor, 
        [FromRoute] int temperatureRecordId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _temperatureService.DeleteEntryById(token, temperatureRecordId);
    }
    
    private void ValidateAuthorization(IHttpContextAccessor httpContextAccessor, out string verifiedAuthorization)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() 
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["HeaderError_Authorization"]);

        verifiedAuthorization = authorization;
    }
}