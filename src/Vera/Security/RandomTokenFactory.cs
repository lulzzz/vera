using System;
using System.Security.Cryptography;

namespace Vera.Security
{
    public class RandomTokenFactory : ITokenFactory
    {
        public string Create()
        {
            using var rng = RandomNumberGenerator.Create();

            var bytes = new byte[32];
            rng.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }
    }
}