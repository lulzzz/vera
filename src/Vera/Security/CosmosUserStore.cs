using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Vera.Security
{
    public sealed class CosmosUserStore : IUserStore
    {
        private readonly Container _container;

        public CosmosUserStore(Container container)
        {
            _container = container;
        }

        public async Task<User> Store(User user)
        {
            var document = await _container.CreateItemAsync(new UserDocument(user));

            return document.Resource.User;
        }

        public async Task<User> GetByCompany(Guid companyId, string username)
        {
            var iterator = _container.GetItemLinqQueryable<UserDocument>(requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(username.ToLower())
                })
                .Where(x => x.User.CompanyId == companyId)
                .Take(1)
                .ToFeedIterator();

            var result = await iterator.ReadNextAsync();

            return result.FirstOrDefault()?.User;
        }

        private class UserDocument
        {
            public UserDocument() { }

            public UserDocument(User user)
            {
                Id = user.Id;
                User = user;
                PartitionKey = user.Username.ToLower();
            }

            public Guid Id { get; set; }
            public User User { get; set; }
            public string PartitionKey { get; set; }
        }
    }
}