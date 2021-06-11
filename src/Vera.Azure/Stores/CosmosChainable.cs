using Microsoft.Azure.Cosmos;
using System;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public class CosmosChainable : IChainable
    {
        private readonly Container _container;
        private readonly string _partitionKeyValue;
        private readonly ChainDocument? _last;

        public CosmosChainable(
            Container container,
            string partitionKeyValue,
            ChainDocument? last
        )
        {
            _container = container;
            _partitionKeyValue = partitionKeyValue;
            _last = last;
        }

        public async Task Append(Signature signature, decimal cumulatedValue = 0)
        {
            var partitionKey = new PartitionKey(_partitionKeyValue);

            var next = new ChainDocument
            {
                Id = Guid.NewGuid(),
                Sequence = NextSequence,
                Signature = signature,
                PartitionKey = _partitionKeyValue,
                CumulatedValue = cumulatedValue
            };

            var tx = _container.CreateTransactionalBatch(partitionKey);
            tx.CreateItem(next);

            if (_last != null)
            {
                next.Previous = _last.Id;
                _last.Next = next.Id;

                // Next got updated so update the previous item in the chain to point to the new one
                tx.ReplaceItem(_last.Id.ToString(), _last);
            }

            using var response = await tx.ExecuteAsync();

            if (!response.IsSuccessStatusCode)
            {
                // TODO: create specific exception
                throw new Exception(response.ErrorMessage);
            }
        }

        public int NextSequence => _last?.Sequence + 1 ?? 1;
        public Signature? Signature => _last?.Signature;
        public decimal CumulatedValue => _last?.CumulatedValue ?? 0;
    }
}
