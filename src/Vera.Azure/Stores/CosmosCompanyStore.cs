using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Vera.Models;

namespace Vera.Stores.Cosmos
{
    public class CosmosCompanyStore : ICompanyStore
    {
        private const string DocumentType = "company";

        private readonly Container _container;

        public CosmosCompanyStore(Container container)
        {
            _container = container;
        }

        public async Task Store(Company company)
        {
            var toCreate = ToDocument(company);

            await _container.CreateItemAsync(
                toCreate,
                new PartitionKey(toCreate.PartitionKey)
            );
        }

        public async Task Update(Company company)
        {
            var document = ToDocument(company);

            await _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey)
            );
        }

        public async Task<Company> GetById(Guid companyId)
        {
            var result = await _container.ReadItemAsync<TypedDocument<Company>>(
                companyId.ToString(), new PartitionKey(companyId.ToString())
            );

            return result.Resource.Value;
        }

        public async Task<Company> GetByName(string name)
        {
            var definition = new QueryDefinition(@"
select top 1 value c[""Value""]
  from c
 where c.Type = @type
  and  c[""Value""].Name = @name")
                .WithParameter("@type", DocumentType)
                .WithParameter("@name", name);

            using var iterator = _container.GetItemQueryIterator<Company>(definition);

            var response = await iterator.ReadNextAsync();

            return response.FirstOrDefault();
        }

        private static TypedDocument<Company> ToDocument(Company company)
        {
            return new(
                c => c.Id,
                c => c.Id.ToString(),
                company,
                DocumentType
            );
        }
    }
}