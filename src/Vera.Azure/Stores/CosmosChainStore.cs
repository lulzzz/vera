using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public class CosmosChainStore : IChainStore
    {
        private readonly Container _container;

        public CosmosChainStore(Container container)
        {
            _container = container;
        }

        public async Task<IChainable> Last(ChainContext context)
        {
            var partitionKeyValue = context.AccountId + ";" + context.Bucket;
            var queryable = _container.GetItemLinqQueryable<ChainDocument>()
                .Where(x => x.Next == null)
                .Where(x => x.PartitionKey == partitionKeyValue);

            using var iterator = queryable.ToFeedIterator();
            var response = await iterator.ReadNextAsync();

            return new CosmosChainable(
                _container,
                partitionKeyValue,
                response.FirstOrDefault()
            );
        }
    }
}