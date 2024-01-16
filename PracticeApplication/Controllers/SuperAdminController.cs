using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticeApplication.Models.Auth;
using PracticeApplication.Services;

namespace PracticeApplication.Controllers;

[Tags("SuperAdmin")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
[Route("/internal-only")]
public class SuperAdminController : Controller
{
    private readonly AuthService _authService;
    
    public SuperAdminController(AuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register-super-admin", Name = "RegisterSuperAdmin")]
    public async Task<RegisterResponse> RegisterSuperAdmin([FromBody] RegistrationPayload payload)
    {
        return await _authService.RegisterUser(payload, "SuperAdmin");
    }
}