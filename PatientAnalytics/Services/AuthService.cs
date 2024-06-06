using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Models.Auth;
using PatientAnalytics.Utils;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Services;

public class AuthService
{
    private readonly IConfiguration _config;
    private readonly Context _context;
    private readonly JwtService _jwtService;
    private readonly UserService _userService;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public AuthService([FromServices] Context context, JwtService jwtService, UserService userService, IConfiguration config, IStringLocalizer<ApiResponseLocalized> localized)
    {
        _config = config;
        _context = context;
        _userService = userService;
        _jwtService = jwtService;
        _localized = localized;
    }

    public LoginResponse Login(LoginPayload loginPayload)
    {
        var user = Authenticate(loginPayload);
        var token = _jwtService.GenerateJwt(user);
        return new LoginResponse(token, user);
    }

    private User Authenticate(LoginPayload loginPayload)
    {
        User? currentUser = _context.Users.FirstOrDefault(user => user.Username == loginPayload.Username);

        if (currentUser == null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status404NotFound, _localized["AuthError_UserNotFound"]); 
        }

        var passwordHash = Password.HashPassword(loginPayload.Password, _config);

        if (currentUser.PasswordHash != passwordHash) 
        { 
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, _localized["AuthError_WrongPassword"]); 
        } 
        return currentUser;
    }
    
    public async Task<RegisterResponse> RegisterUser(string authorization, RegistrationPayload payload, string role)
    {
        var passwordLength = int.Parse(_config["Auth:PasswordLength"] ?? "10");
        var password = payload.Password;
        var validPassword = IsPasswordValid(password);
        var isHashLeaked = await IsPasswordLeaked(password);

        if (isHashLeaked)
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                _localized["AuthError_WeakPassword_IsHashLeaked"]
            );

        if (!validPassword)
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                string.Format(_localized["AuthError_WeakPassword_Validation"], passwordLength)
            );

        var passwordHash = Password.HashPassword(password, _config);
        var user = await _userService.CreateUser(authorization, passwordHash, payload, role);
        var token = _jwtService.GenerateJwt(user);

        return new RegisterResponse(user, token);
    }

    private bool IsPasswordValid(string password)
    {
        var passwordLength = int.Parse(_config["Auth:PasswordLength"] ?? "10");
        var specialCharacterRegex = new Regex("[!@#$%^&*(),.?\":{}|<>]");
        var numberRegex = new Regex("[0-9]");

        if (password.Length < passwordLength) return false;

        if (!specialCharacterRegex.IsMatch(password) || !numberRegex.IsMatch(password)) return false;

        return true;
    }

    private async Task<bool> IsPasswordLeaked(string password)
    {
        var passwordHash = HashString(password);
        passwordHash = passwordHash.ToUpperInvariant();

        var prefix = passwordHash.Substring(0, 5);

        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"https://api.pwnedpasswords.com/range/{prefix}");

        if (response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();
            var pwnedPasswords = responseText.Split('\n');
            var pwnedPassword =
                pwnedPasswords.FirstOrDefault(value => value?.Split(':')[0] == passwordHash.Substring(5), null);
            if (pwnedPassword != null) return true;
        }

        return false;
    }

    private string HashString(string input)
    {
        using var sha1 = SHA1.Create();
        var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}