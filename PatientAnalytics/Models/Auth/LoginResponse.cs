namespace PatientAnalytics.Models.Auth;

public class LoginResponse
{
    public LoginResponse(string token, string refreshToken, User user)
    {
        Token = token;
        RefreshToken = refreshToken;
        User = user;
    }
    
    public string Token { get; private set; }
    public string RefreshToken { get; private set; }
    public User User { get; private set; }
}