using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticeApplication.Models.Auth;
using PracticeApplication.Services;

namespace PracticeApplication.Controllers;

[Tags("Registration")]
[ApiController]
[Authorize(Roles = "SuperAdmin, Admin")]
[Route("/auth/register")]
public class RegistrationController
{
    private readonly AuthService _authService;
    
    public RegistrationController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("admin", Name = "RegisterAdmin")]
    public async Task<RegisterResponse> RegisterAdmin([FromBody] RegistrationPayload payload)
    {
        return await _authService.RegisterUser(payload, "Admin");
    }
    
    [HttpPost("doctor", Name = "RegisterDoctor")]
    public async Task<RegisterResponse> RegisterDoctor([FromBody] RegistrationPayload payload)
    {
        return await _authService.RegisterUser(payload, "Doctor");
    }
}