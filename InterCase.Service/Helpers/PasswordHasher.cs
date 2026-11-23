using System.Security.Cryptography;
using System.Text;

namespace InterCase.Service.Helpers
{
    public static class PasswordHasher
    {
        private const string Algorithm = "PBKDF2-SHA256";
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Derive(password, salt, Iterations);
            return $"{Algorithm}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        public static bool Verify(string password, string passwordHash)
        {
            var parts = passwordHash.Split('$');
            if (parts.Length != 4 || parts[0] != Algorithm || !int.TryParse(parts[1], out var iterations) || iterations <= 0)
                return false;

            if (!TryDecode(parts[2], out var salt) || !TryDecode(parts[3], out var expected))
                return false;

            var actual = Derive(password, salt, iterations);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        private static bool TryDecode(string value, out byte[] bytes)
        {
            var buffer = new byte[value.Length * 3 / 4];
            if (Convert.TryFromBase64String(value, buffer, out var written))
            {
                bytes = buffer[..written];
                return true;
            }

            bytes = [];
            return false;
        }

        private static byte[] Derive(string password, byte[] salt, int iterations) =>
            Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, KeySize);
    }
}
