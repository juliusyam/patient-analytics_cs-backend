using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Services;

namespace PatientAnalytics.Controllers;

[Tags("SuperAdmin")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
[Route("/internal-only")]
public class SuperAdminController : Controller
{
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;

    public SuperAdminController(AuthService authService, JwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }
    
    [HttpPost("register-super-admin", Name = "RegisterSuperAdmin")]
    public async Task<RegisterResponse> RegisterSuperAdmin([FromHeader] string authorization, [FromBody] RegistrationPayload payload)
    {
        var user = _jwtService.GetUserWithJwt(authorization);
        if (user.Role != "SuperAdmin")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You don't have the correct Super Admin authorization");
        }

        return await _authService.RegisterUser(payload, "SuperAdmin");
    }
}