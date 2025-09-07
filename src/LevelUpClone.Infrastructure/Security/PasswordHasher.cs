using System.Security.Cryptography;
using System.Text;

namespace LevelUpClone.Infrastructure.Security
{
    /// <summary>
    /// Hash PBKDF2 (HMACSHA256) com salt embutido. Formato: {iterations}.{saltBase64}.{hashBase64}
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;      // 128 bits
        private const int KeySize = 32;      // 256 bits
        private const int Iterations = 100_000;

        public static string Hash(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            var key = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public static bool Verify(string password, string hash)
        {
            var parts = hash.Split('.', 3);
            if (parts.Length != 3) return false;

            var iter = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var toCheck = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iter,
                HashAlgorithmName.SHA256,
                key.Length);

            return CryptographicOperations.FixedTimeEquals(toCheck, key);
        }
    }
}
