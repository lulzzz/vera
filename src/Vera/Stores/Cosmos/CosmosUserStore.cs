using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using User = Vera.Models.User;

namespace Vera.Stores.Cosmos
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
            var toCreate = new UserDocument(user);

            var document = await _container.CreateItemAsync(
                toCreate,
                new PartitionKey(toCreate.PartitionKey)
            );

            return document.Resource.User;
        }

        public async Task Update(User user)
        {
            var document = new UserDocument(user);

            await _container.ReplaceItemAsync<UserDocument>(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey)
            );
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

            [JsonProperty("id")]
            public Guid Id { get; set; }
            public User User { get; set; }
            public string PartitionKey { get; set; }
        }
    }
}