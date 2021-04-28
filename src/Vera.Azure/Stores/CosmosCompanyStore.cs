using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Vera.Azure.Extensions;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
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

        public Task<Company> GetByName(string name)
        {
            var queryable = _container.GetItemLinqQueryable<TypedDocument<Company>>()
                .Where(x => x.Type == DocumentType && x.Value.Name == name);

            return queryable.FirstOrDefault();
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