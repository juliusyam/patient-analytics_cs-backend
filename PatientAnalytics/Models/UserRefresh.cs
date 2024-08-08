using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PatientAnalytics.Services;
using PatientAnalytics.Utils;

namespace PatientAnalytics.Models;

public class UserRefresh
{
    [Key]
    public int Id { get; init; }
    
    public int UserId { get; private set; }
    
    [ForeignKey("UserId")]
    public User? User { get; init; }

    [MaxLength(100)] 
    public string RefreshTokenHash { get; private set; } = "";
    
    public DateTime RefreshTokenExpiry { get; private set; }
    
    public DateTime DateCreated { get; private set; }

    public static Tuple<string, UserRefresh> CreateRefreshForUser(int userId, IConfiguration config)
    {
        var refreshExpirationTime = Int32.Parse(config["Jwt:RefreshExpirationTime"] ?? "2");

        var refreshToken = JwtService.GenerateRefreshToken();

        var refreshTokenHash = Password.HashPassword(refreshToken, config);

        return Tuple.Create(refreshToken, new UserRefresh
        {
            UserId = userId,
            RefreshTokenHash = refreshTokenHash,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshExpirationTime),
            DateCreated = DateTime.Now
        });
    }
}