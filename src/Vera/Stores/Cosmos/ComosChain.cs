using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Vera.Stores.Cosmos
{
    public sealed class CosmosChain<T> where T : class
    {
        private readonly Container _container;

        public CosmosChain(Container container)
        {
            _container = container;
        }

        public async Task Append(T document, string partitionKeyValue)
        {
            var partitionKey = new PartitionKey(partitionKeyValue);
            var last = await Tail(partitionKey);
            var tx = _container.CreateTransactionalBatch(partitionKey);

            var next = new ChainedDocument<T>(document, partitionKeyValue);

            tx.CreateItem(next);
            
            if (last != null)
            {
                next.Previous = last.Id;
                last.Next = next.Id;
                
                // Next got updated so update the previous item in the chain to point to the new one
                tx.ReplaceItem(last.Id.ToString(), last);
            }

            using var response = await tx.ExecuteAsync();

            if (!response.IsSuccessStatusCode)
            {
                // TODO: create specific exception
                throw new Exception(response.ErrorMessage);
            }
        }

        public async Task<ChainedDocument<T>> Tail(PartitionKey partitionKey)
        {
            var definition = new QueryDefinition("select top 1 * from c where c.Next = null");

            var iterator = _container.GetItemQueryIterator<ChainedDocument<T>>(definition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = partitionKey
                });

            var response = await iterator.ReadNextAsync();

            return response.FirstOrDefault();
        }
    }
}