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

        public Task Store(User user)
        {
            var toCreate = new UserDocument(user);

            return _container.CreateItemAsync(
                toCreate,
                new PartitionKey(toCreate.PartitionKey)
            );
        }

        public async Task Update(User user)
        {
            var document = new UserDocument(user);

            await _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey)
            );
        }

        public async Task<User> GetByCompany(Guid companyId, string username)
        {
            var definition = new QueryDefinition("select top 1 * from c where c.User.CompanyId = @companyId")
                .WithParameter("@companyId", companyId);

            var iterator = _container.GetItemQueryIterator<UserDocument>(definition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(username.ToLower())
                });

            var response = await iterator.ReadNextAsync();

            return response.FirstOrDefault()?.User;
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