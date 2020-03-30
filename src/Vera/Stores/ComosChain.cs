using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Vera.Models;

namespace Vera.Stores
{
    public sealed class CosmosChain<T> where T : class
    {
        private readonly Container _container;

        public CosmosChain(Container container)
        {
            _container = container;
        }

        public async Task<bool> Append(T document, PartitionKey partitionKey)
        {
            var last = await Tail(partitionKey);
            var tx = _container.CreateTransactionalBatch(partitionKey);

            var next = new ChainableDocument<T>(document, partitionKey.ToString());

            tx.CreateItem(next);
            
            if (last != null)
            {
                next.Previous = last.Id;
                last.Next = next.Id;
                
                // Next got updated so update the previous item in the chain to point to the new one
                tx.ReplaceItem(last.Id.ToString(), last);
            }

            using var response = await tx.ExecuteAsync();

            return response.IsSuccessStatusCode;
        }

        public IQueryable<ChainableDocument<T>> Query()
        {
            return _container.GetItemLinqQueryable<ChainableDocument<T>>();
        }
        
        public async Task<ChainableDocument<T>> Tail(PartitionKey partitionKey)
        {
            var iterator = _container.GetItemLinqQueryable<ChainableDocument<T>>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = partitionKey
            })
                .Where(x => x.Next.IsNull())
                .Take(1)
                .ToFeedIterator();

            return (await iterator.ReadNextAsync()).FirstOrDefault();
        }
    }
}