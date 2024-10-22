using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Services;

public class UserService
{
    private readonly Context _context;
    private readonly JwtService _jwtService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public UserService(
        [FromServices] Context context,
        JwtService jwtService,
        IStringLocalizer<ApiResponseLocalized> localized)
    {
        _context = context;
        _jwtService = jwtService;
        _localized = localized;
    }

    public List<User> GetDoctors(string token)
    {
        ValidateIsAdmin(token, out _);

        return _context.Users.Where(u => u.Role == Role.Doctor).ToList();
    }

    public List<User> GetAdmins(string token)
    {
        ValidateIsAdmin(token, out _);

        return _context.Users.Where(u => u.Role == Role.Admin).ToList();
    }

    public List<User> GetSuperAdmins(string token)
    {
        ValidateIsSuperAdmin(token, out _);
        
        return _context.Users.Where(u => u.Role == Role.SuperAdmin).ToList();
    }
    
    public User GetUserById(string token, int userId)
    {
        ValidateIsAdmin(token, out _);
        
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        if (user is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound,
                string.Format(_localized["AuthError_DecodeJwt_UserNotFound"], userId));
        }

        if (user.Role == Role.SuperAdmin)
        {
            ValidateIsSuperAdmin(token, out _);
        }

        return user;
    }

    public async Task<User> EditUserAccountInfo(string token, int userId, UserAccountInfoPayload payload)
    {
        var user = GetUserById(token, userId);
        
        user.UpdateAccountInfo(payload);

        _context.Users.Update(user);

        await _context.SaveChangesAsync();

        return user;
    }
    
    public async Task<IActionResult> DeactivateUser(string token, int userId)
    {
        var user = GetUserById(token, userId);

        UserIsNotRequester(token, user);
        
        if (user.IsDeactivated)
        {
            throw new HttpStatusCodeException(StatusCodes.Status409Conflict,
                string.Format(_localized["UserError_AlreadyDeactivated"], userId));
        }
        
        user.Deactivate();

        _context.Users.Update(user);

        await _context.SaveChangesAsync();

        return new NoContentResult();
    }

    public async Task<IActionResult> ActivateUser(string token, int userId)
    {
        var user = GetUserById(token, userId);

        UserIsNotRequester(token, user);
        
        if (!user.IsDeactivated)
        {
            throw new HttpStatusCodeException(StatusCodes.Status409Conflict,
                string.Format(_localized["UserError_AlreadyActivated"], userId));
        }
        
        user.Activate();

        _context.Users.Update(user);

        await _context.SaveChangesAsync();

        return new NoContentResult();
    }

    private void UserIsNotRequester(string token, User user)
    {
        var requester = _jwtService.GetUserWithJwt(token);

        if (requester.Id == user.Id)
        {
            throw new HttpStatusCodeException(StatusCodes.Status400BadRequest,
                _localized["UserError_RequesterConflict"]);
        }
    }

    private void ValidateIsAdmin(string token, out User verifiedUser)
    {
        var user = _jwtService.GetUserWithJwt(token);

        if (user.Role != Role.SuperAdmin && user.Role != Role.Admin)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                _localized["AuthError_Unauthorized"]);
        }

        verifiedUser = user;
    }
    
    private void ValidateIsSuperAdmin(string token, out User verifiedUser)
    {
        var user = _jwtService.GetUserWithJwt(token);

        if (user.Role != Role.SuperAdmin)
        {
            throw new HttpStatusCodeException(StatusCodes.Status403Forbidden, 
                _localized["AuthError_Unauthorized_SuperAdmin"]);
        }

        verifiedUser = user;
    }
}