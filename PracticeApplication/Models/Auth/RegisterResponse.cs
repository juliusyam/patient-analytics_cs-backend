namespace PracticeApplication.Models.Auth;

public class RegisterResponse
{
    public RegisterResponse(User user, string token)
    {
        User = user;
        Token = token;
    }
    
    public User User { get; private set; }
    public string Token { get; private set; }
}