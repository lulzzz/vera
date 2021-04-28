using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Vera.Azure.Extensions;
using Vera.Stores;
using User = Vera.Models.User;

namespace Vera.Azure.Stores
{
    public sealed class CosmosUserStore : IUserStore
    {
        private const string DocumentType = "user";

        private readonly Container _container;

        public CosmosUserStore(Container container)
        {
            _container = container;
        }

        public Task Store(User user)
        {
            var toCreate = ToDocument(user);

            return _container.CreateItemAsync(
                toCreate,
                new PartitionKey(toCreate.PartitionKey)
            );
        }

        public async Task Update(User user)
        {
            var document = ToDocument(user);

            await _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey)
            );
        }

        public Task<User> GetByCompany(Guid companyId, string username)
        {
            var queryable = _container.GetItemLinqQueryable<TypedDocument<User>>()
                .Where(x => x.Value.CompanyId == companyId 
                            && x.Value.Username == username
                            && x.Type == DocumentType);

            return queryable.FirstOrDefault();
        }

        private static TypedDocument<User> ToDocument(User user)
        {
            return new(
                u => u.Id,
                u => u.CompanyId.ToString(),
                user,
                DocumentType
            );
        }
    }
}