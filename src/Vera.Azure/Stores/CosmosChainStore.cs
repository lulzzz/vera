using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
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
            
            var definition = new QueryDefinition("select top 1 * from c where c.Next = null");

            var iterator = _container.GetItemQueryIterator<ChainDocument>(definition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKeyValue)
                });

            var response = await iterator.ReadNextAsync();

            return new CosmosChainable(
                _container,
                partitionKeyValue,
                response.FirstOrDefault()
            );
        }
    }
}