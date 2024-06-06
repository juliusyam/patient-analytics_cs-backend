using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Services;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Controllers;

[Tags("SuperAdmin")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
[Route("/api/internal-only")]
public class SuperAdminController : Controller
{
    private readonly RegistrationService _registrationService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public SuperAdminController(
        RegistrationService registrationService,
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _registrationService = registrationService;
        _localized = localized;
    }
    
    [HttpPost("register-super-admin", Name = "RegisterSuperAdmin")]
    public async Task<RegisterResponse> RegisterSuperAdmin(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromBody] RegistrationPayload payload)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return await _registrationService.RegisterUser(authorization, payload, "SuperAdmin");
    }

    private void ValidateAuthorization(IHttpContextAccessor httpContextAccessor, out string verifiedAuthorization)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() 
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["HeaderError_Authorization"]);

        verifiedAuthorization = authorization;
    }
}