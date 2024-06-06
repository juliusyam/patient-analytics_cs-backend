using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;


namespace PatientAnalytics.Utils;

public static class Password
{
    public static string HashPassword(string password, IConfiguration configuration)
    {
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.UTF8.GetBytes(configuration["Auth:Salt"] ?? string.Empty),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return hashed;
    }

    public static string GeneratePassword(int length = 64)
    {
        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+";
        var random = new Random();

        return new string(Enumerable.Repeat(characters, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    public static bool IsPasswordValid(string password, IConfiguration configuration)
    {
        var passwordLength = int.Parse(configuration["Auth:PasswordLength"] ?? "10");
        var specialCharacterRegex = new Regex("[!@#$%^&*(),.?\":{}|<>]");
        var numberRegex = new Regex("[0-9]");

        if (password.Length < passwordLength) return false;

        if (!specialCharacterRegex.IsMatch(password) || !numberRegex.IsMatch(password)) return false;

        return true;
    }
    
    public static async Task<bool> IsPasswordLeaked(string password)
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
    
    private static string HashString(string input)
    {
        using var sha1 = SHA1.Create();
        var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}