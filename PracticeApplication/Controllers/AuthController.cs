using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticeApplication.Models.Auth;
using PracticeApplication.Services;

namespace PracticeApplication.Controllers;

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
    
    [HttpPost(template: "register", Name = "Register")]
    public async Task<RegisterResponse> Register([FromBody] RegistrationPayload payload)
    {
        return await _authService.RegisterUser(payload);
    }
    
    [HttpPost(template: "login", Name = "Login")]
    public LoginResponse Login([FromBody] LoginPayload loginPayload)
    {
        return _authService.Login(loginPayload);
    }
}