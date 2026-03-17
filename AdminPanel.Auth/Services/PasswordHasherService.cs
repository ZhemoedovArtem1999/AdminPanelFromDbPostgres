using AdminPanel.Domain.Interfaces.Auth;
using System.Security.Cryptography;

namespace AdminPanel.Auth.Services;

public class PasswordHasherService : IPasswordHasherService
{
    private const int Iterations = 10000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string HashPassword(string password, out string salt)
    {
        byte[] saltBytes = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        salt = Convert.ToBase64String(saltBytes);

        using var deriveBytes = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var hashBytes = deriveBytes.GetBytes(HashSize);
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] hashBytes = Convert.FromBase64String(hash);

        using var deriveBytes = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var testHash = deriveBytes.GetBytes(HashSize);
        return CryptographicOperations.FixedTimeEquals(hashBytes, testHash);
    }
}
