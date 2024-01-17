namespace PatientAnalytics.Models.Auth;

public class LoginResponse
{
    public LoginResponse(string token, User user)
    {
        Token = token;
        User = user;
    }
    
    public string Token { get; private set; }
    public User User { get; private set; }
}