using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using Vera.Models;

namespace Vera.Stores
{
    public class CosmosCompanyStore : ICompanyStore
    {
        private readonly Container _container;

        public CosmosCompanyStore(Container container)
        {
            _container = container;
        }

        public async Task<Company> Store(Company company)
        {
            var toCreate = new CompanyDocument(company);

            var document = await _container.CreateItemAsync(
                toCreate,
                new PartitionKey(toCreate.PartitionKey)
            );

            return document.Resource.Company;
        }

        public async Task<Company> Update(Company company)
        {
            var document = new CompanyDocument(company);

            await _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey)
            );

            return company;
        }

        public async Task<Company> GetByName(string name)
        {
            var iterator = _container.GetItemLinqQueryable<CompanyDocument>(requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(name.ToLower())
                })
                .Take(1)
                .ToFeedIterator();

            var response = await iterator.ReadNextAsync();

            return response.FirstOrDefault()?.Company;
        }

        public async Task<Company> Get(Guid companyId)
        {
            // TODO(kevin): this does a cross partition query
            var document = await _container.ReadItemAsync<CompanyDocument>(
                companyId.ToString(),
                PartitionKey.None
            );

            return document.Resource.Company;
        }

        private class CompanyDocument
        {
            public CompanyDocument() { }

            public CompanyDocument(Company company)
            {
                Id = company.Id;
                Company = company;

                // TODO(kevin): better to use the Id as the partition key instead?
                // have that available on the user and feels like a more logical option
                PartitionKey = company.Name.ToLower();
            }

            [JsonProperty("id")]
            public Guid Id { get; set; }
            public Company Company { get; set; }
            public string PartitionKey { get; set; }
        }
    }
}