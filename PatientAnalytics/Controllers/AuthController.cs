using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Services;

namespace PatientAnalytics.Controllers;

[Tags("Authentication")]
[ApiController]
[AllowAnonymous]
[Route("/auth")]
public class AuthController : Controller
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost(template: "login", Name = "Login")]
    public LoginResponse Login([FromBody] LoginPayload loginPayload)
    {
        return _authService.Login(loginPayload);
    }
}