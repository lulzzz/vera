using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Vera.Security
{
    public class CosmosCompanyStore : ICompanyStore
    {
        private readonly Container _container;

        public CosmosCompanyStore(Container container)
        {
            _container = container;
        }

        public Task Store(Company company)
        {
            return _container.CreateItemAsync(company);
        }

        public async Task<Company> GetByName(string name)
        {
            var iterator = _container.GetItemLinqQueryable<Company>()
                .Where(c => c.Name == name)
                .Take(1)
                .ToFeedIterator();

            var response = await iterator.ReadNextAsync();

            return response.FirstOrDefault();
        }
    }
}