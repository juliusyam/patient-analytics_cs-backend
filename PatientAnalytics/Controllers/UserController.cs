using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Services;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Controllers;

[Tags("Users")]
[ApiController]
[Authorize(Roles = $"{nameof(Role.SuperAdmin)}, {nameof(Role.Admin)}")]
[Route("/api")]
public class UserController
{
    private readonly UserService _userService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public UserController(
        UserService userService,
        IStringLocalizer<ApiResponseLocalized> localized)
    {
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

    [Authorize(Roles = $"{nameof(Role.SuperAdmin)}")]
    [HttpGet("super-admins", Name = "GetSuperAdmins")]
    public List<User> GetSuperAdmins(
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return _userService.GetSuperAdmins(authorization);
    }

    [HttpGet("users/{userId:int}", Name = "GetUserById")]
    public User GetUserBydId(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int userId)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return _userService.GetUserById(authorization, userId);
    }
    
    [HttpPut("users/{userId:int}", Name = "EditUserAccountInfo")]
    public async Task<User> EditUserAccountInfo(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int userId,
        [FromBody] UserAccountInfoPayload payload)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return await _userService.EditUserAccountInfo(authorization, userId, payload);
    }

    [HttpPut("users/{userId:int}/deactivate", Name = "DeactivateUser")]
    public async Task<IActionResult> DeactivateUser(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int userId)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return await _userService.DeactivateUser(authorization, userId);
    }
    
    [HttpPut("users/{userId:int}/activate", Name = "ActivateUser")]
    public async Task<IActionResult> ActivateUser(
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] int userId)
    {
        ValidateAuthorization(httpContextAccessor, out var authorization);

        return await _userService.ActivateUser(authorization, userId);
    }

    private void ValidateAuthorization(IHttpContextAccessor httpContextAccessor, out string verifiedAuthorization)
    {
        var authorization = httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString() 
                            ?? throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["HeaderError_Authorization"]);

        verifiedAuthorization = authorization;
    }
}