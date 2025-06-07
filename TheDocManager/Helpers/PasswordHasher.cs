using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Text;

namespace TheDocManager.Helpers
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int Iterations = 100_000;
        private const int HashSize = 64;

        public static string HashPassword(string password)
        {
#if DEBUG
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
#endif
            var salt = GenerateSalt();
            var hash = Pbkdf2(password, salt, Iterations, HashSize);
            return $"{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
#if DEBUG
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            ArgumentException.ThrowIfNullOrWhiteSpace(storedHash);
#endif
            var parts = storedHash.Split(':');
            if (parts.Length != 3)
                return false;

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[2]);

            var testHash = Pbkdf2(password, salt, iterations, storedHashBytes.Length);
            return CompareHashes(storedHashBytes, testHash);
        }

        private static byte[] GenerateSalt()
        {
            var rng = new Org.BouncyCastle.Security.SecureRandom();
            var salt = new byte[SaltSize];
            rng.NextBytes(salt);
            return salt;
        }

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int length)
        {
#if DEBUG
            ArgumentException.ThrowIfNullOrEmpty(password);
            ArgumentNullException.ThrowIfNull(salt);
#endif
            var gen = new Pkcs5S2ParametersGenerator(new Sha512Digest());
            gen.Init(Encoding.UTF8.GetBytes(password), salt, iterations);
            var key = (KeyParameter)gen.GenerateDerivedMacParameters(length * 8);
            return key.GetKey();
        }

        private static bool CompareHashes(byte[] a, byte[] b)
        {
#if DEBUG
            ArgumentNullException.ThrowIfNull(a);
            ArgumentNullException.ThrowIfNull(b);
#endif
            if (a.Length != b.Length) return false;
            bool result = true;
            for (int i = 0; i < a.Length; i++)
                result &= a[i] == b[i];
            return result;
        }
    }
}
