using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Vera.Security
{
    public sealed class CosmosUserStore : IUserStore
    {
        private readonly Container _container;
        private readonly IPasswordStrategy _passwordStrategy;

        public CosmosUserStore(Container container, IPasswordStrategy passwordStrategy)
        {
            _container = container;
            _passwordStrategy = passwordStrategy;
        }

        public async Task<User> Store(UserToCreate toCreate)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = toCreate.Username,
                Authentication = _passwordStrategy.Encrypt(toCreate.Password),
                Type = toCreate.Type
            };

            await _container.CreateItemAsync(user);

            return user;
        }

        public Task<User> Get(string username, Guid companyId)
        {
            throw new NotImplementedException();
        }
    }
}