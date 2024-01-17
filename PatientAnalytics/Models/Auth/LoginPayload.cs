namespace PatientAnalytics.Models.Auth;

public class LoginPayload
{
    public LoginPayload(string username, string password)
    {
        Username = username;
        Password = password;
    }
    public string Username { get; private set; }
    public string Password { get; private set; }
}