using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public class CosmosAccountStore : IAccountStore
    {
        private const string DocumentType = "account";

        private readonly Container _container;

        public CosmosAccountStore(Container container)
        {
            _container = container;
        }

        public Task Store(Account account)
        {
            var document = ToDocument(account);

            return _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));
        }

        public Task Update(Account account)
        {
            var document = ToDocument(account);

            return _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey)
            );
        }

        public async Task<Account> Get(Guid companyId, Guid accountId)
        {
            var document = await _container.ReadItemAsync<TypedDocument<Account>>(
                accountId.ToString(),
                new PartitionKey(companyId.ToString())
            );

            return document.Resource.Value;
        }

        public async Task<ICollection<Account>> GetByCompany(Guid companyId)
        {
            var definition = new QueryDefinition(@"
select value a[""Value""] 
from a 
where a.Type = @type")
                .WithParameter("@type", DocumentType);

            using var iterator = _container.GetItemQueryIterator<Account>(definition, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(companyId.ToString())
            });

            var accounts = new List<Account>();

            while (iterator.HasMoreResults)
            {
                accounts.AddRange(
                    from result in await iterator.ReadNextAsync()
                    select result
                );
            }

            return accounts;
        }

        private static TypedDocument<Account> ToDocument(Account account)
        {
            return new(
                a => a.Id,
                a => a.CompanyId.ToString(),
                account,
                DocumentType
            );
        }
    }
}