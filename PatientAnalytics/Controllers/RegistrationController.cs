using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Services;

namespace PatientAnalytics.Controllers;

[Tags("Registration")]
[ApiController]
[Authorize(Roles = "SuperAdmin, Admin")]
[Route("/auth/register")]
public class RegistrationController
{
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;

    public RegistrationController(AuthService authService, JwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    [HttpPost("admin", Name = "RegisterAdmin")]
    public async Task<RegisterResponse> RegisterAdmin([FromHeader] string authorization, [FromBody] RegistrationPayload payload)
    {
        var user = _jwtService.GetUserWithJwt(authorization);
        if (user.Role != "SuperAdmin" && user.Role != "Admin")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You don't have the correct authorization");
        }
        
        return await _authService.RegisterUser(payload, "Admin");
    }
    
    [HttpPost("doctor", Name = "RegisterDoctor")]
    public async Task<RegisterResponse> RegisterDoctor([FromHeader] string authorization, [FromBody] RegistrationPayload payload)
    {
        var user = _jwtService.GetUserWithJwt(authorization);
        if (user.Role != "SuperAdmin" && user.Role != "Admin")
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                "You don't have the correct authorization");
        }

        return await _authService.RegisterUser(payload, "Doctor");
    }
}