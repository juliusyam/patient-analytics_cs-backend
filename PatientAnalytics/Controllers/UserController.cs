using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Services;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Controllers;

[Tags("Users")]
[ApiController]
[Authorize(Roles = "SuperAdmin, Admin")]
[Route("/api")]
public class UserController
{
    private readonly RegistrationService _registrationService;
    private readonly UserService _userService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public UserController(
        RegistrationService registrationService,
        UserService userService,
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _registrationService = registrationService;
        _userService = userService;
        _localized = localized;
    }

    [HttpGet("admins", Name = "GetAdmins")]
    public List<User> GetAdmins(
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return _userService.GetAdmins(authorization);
    }

    [HttpGet("doctors", Name = "GetDoctors")]
    public List<User> GetDoctors(
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return _userService.GetDoctors(authorization);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("super-admins", Name = "GetSuperAdmins")]
    public List<User> GetSuperAdmins(
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return _userService.GetSuperAdmins(authorization);
    }

    private void ValidateAuthorization(IHttpContextAccessor httpContextAccessor, out string verifiedAuthorization)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() 
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["HeaderError_Authorization"]);

        verifiedAuthorization = authorization;
    }
}