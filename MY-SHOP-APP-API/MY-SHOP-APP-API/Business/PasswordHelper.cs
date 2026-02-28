using System;
using System.Security.Cryptography;

namespace MY_SHOP_APP_API.Business
{
    // Simple PBKDF2 password hashing helper
    internal static class PasswordHelper
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            // format: iter.salt.hash (base64)
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyHashedPassword(string? hashed, string password)
        {
            if (string.IsNullOrEmpty(hashed)) return false;
            var parts = hashed.Split('.', 3);
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out var iter)) return false;
            var salt = Convert.FromBase64String(parts[1]);
            var hash = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256);
            var computed = pbkdf2.GetBytes(hash.Length);
            return CryptographicOperations.FixedTimeEquals(computed, hash);
        }
    }
}
