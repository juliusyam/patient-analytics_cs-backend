using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace PracticeApplication.Utils;

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
}