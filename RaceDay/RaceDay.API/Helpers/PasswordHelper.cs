using System.Security.Cryptography;
using System.Text;

namespace RaceDay.API.Helpers
{
    public static class PasswordHelper
    {
        // Hashes the password using SHA256 with a static salt
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "RaceDaySalt2026"));
            return Convert.ToBase64String(hashedBytes);
        }

        // Verifies the input password against the stored hash
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}