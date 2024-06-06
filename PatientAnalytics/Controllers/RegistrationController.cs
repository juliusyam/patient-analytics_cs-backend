using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Services;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Controllers;

[Tags("Registration")]
[ApiController]
[Authorize(Roles = "SuperAdmin, Admin")]
[Route("/api/auth/register")]
public class RegistrationController
{
    private readonly RegistrationService _registrationService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public RegistrationController(
        RegistrationService registrationService,
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _registrationService = registrationService;
        _localized = localized;
    }

    [HttpPost("admin", Name = "RegisterAdmin")]
    public async Task<RegisterResponse> RegisterAdmin(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromBody] RegistrationPayload payload)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);
        
        return await _registrationService.RegisterUser(authorization, payload, "Admin");
    }
    
    [HttpPost("doctor", Name = "RegisterDoctor")]
    public async Task<RegisterResponse> RegisterDoctor(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromBody] RegistrationPayload payload)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);
        
        return await _registrationService.RegisterUser(authorization, payload, "Doctor");
    }

    private void ValidateAuthorization(IHttpContextAccessor httpContextAccessor, out string verifiedAuthorization)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() 
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["HeaderError_Authorization"]);

        verifiedAuthorization = authorization;
    }
}