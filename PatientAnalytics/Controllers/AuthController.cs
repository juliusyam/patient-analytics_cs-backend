using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Services;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Controllers;

[Tags("Authentication")]
[ApiController]
[AllowAnonymous]
[Route("/api/auth")]
public class AuthController : Controller
{
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public AuthController(
        AuthService authService, 
        JwtService jwtService, 
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _authService = authService;
        _jwtService = jwtService;
        _localized = localized;
    }

    [HttpPost("login", Name = "Login")]
    public async Task<LoginResponse> Login([FromBody] LoginPayload loginPayload)
    {
        return await _authService.Login(loginPayload);
    }

    [HttpPost("refresh", Name = "Refresh")]
    public async Task<LoginResponse> Refresh(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromBody] RefreshPayload refreshPayload)
    {
        ValidateAuthorization(httpContextAccessor, out var token);
        
        return await _authService.Refresh(token, refreshPayload);
    }

    [Authorize]
    [HttpDelete("logout", Name = "Logout")]
    public async Task<IActionResult> Logout(
        [FromServices] IHttpContextAccessor httpContextAccessor, 
        [FromBody] RefreshPayload refreshPayload)
    {
        ValidateAuthorization(httpContextAccessor, out var token);

        return await _authService.Logout(token, refreshPayload.RefreshToken);
    }
    
    private void ValidateAuthorization(IHttpContextAccessor httpContextAccessor, out string verifiedAuthorization)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() 
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["HeaderError_Authorization"]);

        _jwtService.TokenFormatIsValid(authorization);
        
        verifiedAuthorization = authorization;
    }
}