using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PracticeApplication.Middleware;
using PracticeApplication.Models;

namespace PracticeApplication.Services;

public class JwtService
{
    private readonly IConfiguration _config;
    private readonly Context _context;

    public JwtService([FromServices] Context context, IConfiguration config)
    {
        _config = config;
        _context = context;
    }
    
    public string GenerateJwt(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Username),
            new Claim(ClaimTypes.Sid, user.Id.ToString())
        };

        var expirationTime = Int32.Parse(_config["Jwt:ExpirationTime"] ?? "1");

        var token = new JwtSecurityToken(_config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddDays(expirationTime),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public User DecodeJWT(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        
        var jsonToken = handler.ReadJwtToken(token.Replace("Bearer ", ""));

        var claim = jsonToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid");
        var userIdString = claim?.Value;
        
        if (userIdString is null || !int.TryParse(userIdString, out var userId))
        {
            throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, "Unable to decode User ID from token");
        }

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        if (user is null)
        {
            throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, $"Unable to find user with id: {userId}");
        }

        return user;
    }
    
}