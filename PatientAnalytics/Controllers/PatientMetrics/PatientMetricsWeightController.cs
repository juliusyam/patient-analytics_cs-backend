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
public class PatientMetricsWeightController : Controller
{
    private readonly PatientMetricsWeightService _weightService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public PatientMetricsWeightController(
        PatientMetricsWeightService weightService,
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _weightService = weightService;
        _localized = localized;
    }

    [HttpPost("patients/{patientId:int}/weights", Name = "CreateWeightEntry")]
    public async Task<PatientWeight> CreateEntry(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int patientId,
        [FromBody] PatientWeightPayload payload)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _weightService.CreateEntry(token, patientId, payload);
    }

    [HttpGet("patients/{patientId:int}/weights", Name = "GetPatientWeights")]
    public async Task<List<PatientWeight>> GetPatientWeights(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int patientId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _weightService.GetPatientWeights(token, patientId);
    }

    [HttpGet("weights/{weightRecordId:int}", Name = "GetWeightEntryById")]
    public PatientWeight GetEntryById(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int weightRecordId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return _weightService.GetEntryById(token, weightRecordId);
    }

    [HttpDelete("weights/{weightRecordId:int}", Name = "DeleteWeightEntryById")]
    public async Task<IActionResult> DeleteEntryById(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int weightRecordId)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _weightService.DeleteEntryById(token, weightRecordId);
    }

    private void ValidateAuthorization(IHttpContextAccessor httpContextAccessor, out string verifiedAuthorization)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString()
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                                _localized["HeaderError_Authorization"]);

        verifiedAuthorization = authorization;
    }
}