using System;
using System.Security.Cryptography;

namespace pm05.Services
{
    public static class PasswordHasher
    {
        private const int SaltSize = 32;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string GenerateSalt()
        {
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return Convert.ToBase64String(salt);
        }

        public static string ComputeHash(string password, string salt)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (salt == null) throw new ArgumentNullException(nameof(salt));

            byte[] saltBytes;
            try
            {
                saltBytes = Convert.FromBase64String(salt);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Salt is not a valid Base64 string.", nameof(salt), ex);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations))
            {
                var hash = pbkdf2.GetBytes(HashSize);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
