using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using User = Vera.Models.User;

namespace Vera.Stores.Cosmos
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

        public async Task<User> GetByCompany(Guid companyId, string username)
        {
            var definition = new QueryDefinition(@"
select top 1 value c[""Value""]
  from c 
where c[""Value""].Username = @username
  and c.Type = @type")
                .WithParameter("@username", username)
                .WithParameter("@type", DocumentType);

            var iterator = _container.GetItemQueryIterator<User>(
                definition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(companyId.ToString())
                }
            );

            var response = await iterator.ReadNextAsync();

            return response.FirstOrDefault();
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