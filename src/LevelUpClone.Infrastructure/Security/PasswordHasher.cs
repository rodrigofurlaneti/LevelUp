using System;
using System.Security.Cryptography;
using System.Text;

namespace LevelUpClone.Infrastructure.Security
{
    public sealed class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;     // 128 bits
        private const int KeySize = 32;     // 256 bits
        private const int Iterations = 100_000;
        private const string Version = "v1";
        private static readonly HashAlgorithmName Algo = HashAlgorithmName.SHA256;

        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            password = password.Normalize(NormalizationForm.FormKC);

            Span<byte> salt = stackalloc byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                password: Encoding.UTF8.GetBytes(password),
                salt: salt.ToArray(),
                iterations: Iterations,
                hashAlgorithm: Algo,
                outputLength: KeySize // <-- nome correto do parâmetro
            );

            var saltB64 = Convert.ToBase64String(salt);
            var keyB64 = Convert.ToBase64String(key);

            return $"{Version}|{Algo.Name}|{Iterations}|{saltB64}|{keyB64}";
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            password = password.Normalize(NormalizationForm.FormKC);

            var parts = hash.Split('|');
            if (parts.Length != 5) return false;

            var version = parts[0];
            var algoName = parts[1];
            if (!int.TryParse(parts[2], out int iters) || iters <= 0) return false;

            if (!TryFromBase64(parts[3], out var salt) || !TryFromBase64(parts[4], out var key))
                return false;

            if (!string.Equals(version, Version, StringComparison.Ordinal) ||
                !string.Equals(algoName, Algo.Name, StringComparison.Ordinal))
                return false;

            byte[] toCheck = Rfc2898DeriveBytes.Pbkdf2(
                password: Encoding.UTF8.GetBytes(password),
                salt: salt,
                iterations: iters,
                hashAlgorithm: new HashAlgorithmName(algoName),
                outputLength: key.Length // <-- nome correto do parâmetro
            );

            var equals = CryptographicOperations.FixedTimeEquals(toCheck, key);
            Array.Clear(toCheck, 0, toCheck.Length);
            return equals;
        }

        public bool NeedsUpgrade(string hash)
        {
            var parts = hash?.Split('|');
            if (parts is null || parts.Length != 5) return true;
            if (!int.TryParse(parts[2], out int iters)) return true;
            return iters < Iterations;
        }

        private static bool TryFromBase64(string s, out byte[] bytes)
        {
            try { bytes = Convert.FromBase64String(s); return true; }
            catch { bytes = Array.Empty<byte>(); return false; }
        }
    }
}