using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace TafsilkPlatform.Utility.Security
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] subkey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 100_000, 32);
            var outputBytes = new byte[1 + 16 + 32];
            outputBytes[0] = 0x01; // format marker
            Buffer.BlockCopy(salt, 0, outputBytes, 1, 16);
            Buffer.BlockCopy(subkey, 0, outputBytes, 17, 32);
            return Convert.ToBase64String(outputBytes);
        }

        public static bool Verify(string hashedPassword, string password)
        {
            try
            {
                var decoded = Convert.FromBase64String(hashedPassword);
                if (decoded.Length != 49 || decoded[0] != 0x01) return false;
                var salt = new byte[16];
                Buffer.BlockCopy(decoded, 1, salt, 0, 16);
                var expected = new byte[32];
                Buffer.BlockCopy(decoded, 17, expected, 0, 32);
                var actual = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 100_000, 32);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch { return false; }
        }
    }
}
