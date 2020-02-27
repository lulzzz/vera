using System.Linq;
using System.Security.Cryptography;

namespace Vera.Security
{
    public sealed class Pbkdf2PasswordStrategy : IPasswordStrategy
    {
        public Authentication Encrypt(string password)
        {
            const int iterations = 10_000;
            const int saltSize = 16;
            const int hashSize = 16;

            var salt = new byte[saltSize];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512);
            var hash = pbkdf2.GetBytes(hashSize);

            var authentication = new Authentication
            {
                Method = "PBKDF2:SHA512",
                Hash = hash,
                Salt = salt,
                Iterations = iterations
            };

            return authentication;
        }

        public bool Verify(string password, Authentication authentication)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                authentication.Salt,
                authentication.Iterations,
                HashAlgorithmName.SHA512
            );

            var result = pbkdf2.GetBytes(authentication.Hash.Length);

            return result.SequenceEqual(authentication.Hash);
        }
    }
}