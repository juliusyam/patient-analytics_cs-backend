using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Utils;
using PatientAnalytics.Utils.Localization;

namespace PatientAnalytics.Services;

public class JwtService
{
    private readonly IConfiguration _config;
    private readonly Context _context;
    private readonly IStringLocalizer<ApiResponseLocalized> _localized;

    public JwtService([FromServices] Context context, IConfiguration config, IStringLocalizer<ApiResponseLocalized> localized)
    {
        _config = config;
        _context = context;
        _localized = localized;
    }
    
    public string GenerateJwt(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Username),
            new Claim(ClaimTypes.Sid, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var expirationTime = Int32.Parse(_config["Jwt:ExpirationTime"] ?? "1");

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddDays(expirationTime),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];

        using var generator = RandomNumberGenerator.Create();
        
        generator.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    public void TokenFormatIsValid(string token)
    {
        var formattedToken = token.Replace("Bearer ", "");
        
        var handler = new JwtSecurityTokenHandler();
        
        if (!handler.CanReadToken(formattedToken))
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                _localized["AuthError_TokenFormatInvalid"]);
        }
    }

    public ClaimsPrincipal DecodeJwt(string token)
    {
        ValidateToken(token, out var claimsPrincipal);
        
        return new ClaimsPrincipal(new ClaimsIdentity(claimsPrincipal.Claims,  "Custom Authentication"));
    }

    public User GetUserWithJwt(string token)
    {
        ValidateToken(token, out var claimsPrincipal);

        GetUserFromDatabase(claimsPrincipal, out var user);

        return user;
    }

    public User GetUserWithExpiredJwt(string token, string refreshToken)
    {
        GetPrincipalFromExpiredToken(token, out var claimsPrincipal);
        
        GetUserFromDatabase(claimsPrincipal, out var user);

        var refreshTokenHash = Password.HashPassword(refreshToken, _config);
        
        var refreshRecord = user.UserRefreshes.FirstOrDefault(ur => ur.RefreshTokenHash == refreshTokenHash);

        if (refreshRecord is null || refreshRecord.RefreshTokenExpiry < DateTime.UtcNow)
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized,
                _localized["AuthError_Refresh_InvalidOrExpired"]);
        }

        return user;
    }

    private void GetUserFromDatabase(ClaimsPrincipal claimsPrincipal, out User verifiedUser)
    {
        var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid);
        var userIdString = claim?.Value;
        
        if (userIdString is null || !int.TryParse(userIdString, out var userId))
        {
            throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, 
                _localized["AuthError_DecodeJwt_Parse"]);
        }

        var user = _context.Users
            .Include(u => u.UserRefreshes)
            .FirstOrDefault(u => u.Id == userId);

        verifiedUser = user ?? throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, 
            string.Format(_localized["AuthError_DecodeJwt_UserNotFound"], userId));;
    }

    private void ValidateToken(string token, out ClaimsPrincipal claimsPrincipal)
    {
        TokenFormatIsValid(token);
        
        var formattedToken = token.Replace("Bearer ", "");
        
        var validation = new TokenValidationParameters
        {
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)),
            ValidateLifetime = true,
            ClockSkew = new TimeSpan(0, 0, 5)
        };

        try
        {
            claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(formattedToken, validation, out _);
        }
        catch (SecurityTokenExpiredException exception)
        {
            throw new HttpStatusCodeException(StatusCodes.Status401Unauthorized, exception.Message);
        }
    }

    private void GetPrincipalFromExpiredToken(string token, out ClaimsPrincipal claimsPrincipal)
    {
        var formattedToken = token.Replace("Bearer ", "");
        
        var validation = new TokenValidationParameters
        {
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)),
            ValidateLifetime = false, // Do not validate life time as token is expired
            ClockSkew = new TimeSpan(0, 0, 5)
        };

        claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(formattedToken, validation, out _);
    }
}