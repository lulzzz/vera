using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;

namespace Vera.Security
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

        private class CompanyDocument
        {
            public CompanyDocument() { }

            public CompanyDocument(Company company)
            {
                Id = company.Id;
                Company = company;
                PartitionKey = company.Name.ToLower();
            }

            [JsonProperty("id")]
            public Guid Id { get; set; }
            public Company Company { get; set; }
            public string PartitionKey { get; set; }
        }
    }
}