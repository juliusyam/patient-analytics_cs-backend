using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
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
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public RegistrationController(AuthService authService, JwtService jwtService, IStringLocalizer<ApiResponseLocalized> localized)
    {
        _authService = authService;
        _jwtService = jwtService;
        _localized = localized;
    }

    [HttpPost("admin", Name = "RegisterAdmin")]
    public async Task<RegisterResponse> RegisterAdmin([FromServices] IHttpContextAccessor httpContextAccessor, [FromBody] RegistrationPayload payload)
    {
        ValidateAdmin(httpContextAccessor, out var authorization, out _);
        
        return await _authService.RegisterUser(authorization, payload, "Admin");
    }
    
    [HttpPost("doctor", Name = "RegisterDoctor")]
    public async Task<RegisterResponse> RegisterDoctor([FromServices] IHttpContextAccessor httpContextAccessor, [FromBody] RegistrationPayload payload)
    {
        ValidateAdmin(httpContextAccessor, out var authorization, out _);
        
        return await _authService.RegisterUser(authorization, payload, "Doctor");
    }

    private void ValidateAdmin(IHttpContextAccessor httpContextAccessor, out string verifiedToken, out User verifiedUser)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() 
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["HeaderError_Authorization"]);
        
        var user = _jwtService.GetUserWithJwt(authorization);

        if (user.Role != "SuperAdmin" && user.Role != "Admin")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                _localized["AuthError_Unauthorized"]);
        }

        verifiedToken = authorization;
        verifiedUser = user;
    }
}