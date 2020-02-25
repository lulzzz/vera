using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Vera.Security
{
    public sealed class CosmosUserStore : IUserStore
    {
        public CosmosUserStore()
        {
        }

        public Task Store(UserToCreate toCreate)
        {
            // TODO(kevin): move hashing etc. out of this class, it should just store the user
            const int iterations = 100_000;
            const int saltSize = 16;
            const int hashSize = 16;

            var salt = new byte[saltSize];

            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(salt);

            var pbkdf2 = new Rfc2898DeriveBytes(toCreate.Password, salt, iterations, HashAlgorithmName.SHA512);
            var hash = pbkdf2.GetBytes(hashSize);

            var authentication = new Authentication
            {
                Method = "PBKDF2:SHA512",
                Hash = hash,
                Salt = salt
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = toCreate.Username,
                Authentication = authentication,
                Type = UserType.Admin
            };

            // TODO: get company

            // TODO: add user to company
            // TODO: store user in cosmos

            return Task.CompletedTask;
        }

        public Task<User> Get(string username, Guid companyId)
        {
            throw new NotImplementedException();
        }
    }
}