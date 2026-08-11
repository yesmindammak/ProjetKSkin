using System;
using System.Security.Cryptography;

namespace LoginRegisterApp.Helpers
{
    public static class PasswordHelper
    {
        // Turns a plain-text password into a salted hash that's safe to store in the DB.
        // Format stored: "salt.hash" (both Base64), so we can verify it later.
        public static string HashPassword(string plainPassword)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password: plainPassword,
                salt: salt,
                iterations: 100_000,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: 32);

            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        // Compares a plain-text password (typed at login) against the stored "salt.hash" string.
        public static bool VerifyPassword(string plainPassword, string storedHash)
        {
            string[] parts = storedHash.Split('.');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[1]);

            byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password: plainPassword,
                salt: salt,
                iterations: 100_000,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: 32);

            return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHash);
        }

        // Generates a random 10-character password for new users, e.g. "aT9!kLpQ2z"
        public static string GenerateRandomPassword(int length = 10)
        {
            const string chars = "abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789!@#$";
            var result = new char[length];
            byte[] randomBytes = RandomNumberGenerator.GetBytes(length);

            for (int i = 0; i < length; i++)
                result[i] = chars[randomBytes[i] % chars.Length];

            return new string(result);
        }
    }
}
