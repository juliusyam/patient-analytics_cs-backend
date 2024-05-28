using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
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
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public SuperAdminController(AuthService authService, JwtService jwtService, IStringLocalizer<ApiResponseLocalized> localized)
    {
        _authService = authService;
        _jwtService = jwtService;
        _localized = localized;
    }
    
    [HttpPost("register-super-admin", Name = "RegisterSuperAdmin")]
    public async Task<RegisterResponse> RegisterSuperAdmin([FromServices] IHttpContextAccessor httpContextAccessor, [FromBody] RegistrationPayload payload)
    {
        ValidateAdmin(httpContextAccessor, out _);

        return await _authService.RegisterUser(payload, "SuperAdmin");
    }

    private void ValidateAdmin(IHttpContextAccessor httpContextAccessor, out User verifiedUser)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() 
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["HeaderError_Authorization"]);
        
        var user = _jwtService.GetUserWithJwt(authorization);

        if (user.Role != "SuperAdmin")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                _localized["AuthError_Unauthorized_SuperAdmin"]);
        }

        verifiedUser = user;
    }
}