using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using Vera.Models;

namespace Vera.Stores
{
    public sealed class CosmosChain
    {
        private readonly Container _container;
        private readonly string _partitionKey;

        public CosmosChain(Container container, string partitionKey)
        {
            _container = container;
            _partitionKey = partitionKey;
        }

        public async Task<bool> Append(Guid documentId)
        {
            var last = await Tail();

            ChainLink next;

            if (last == null)
            {
                next = new ChainLink
                {
                    Id = Guid.NewGuid(),
                    PartitionKey = _partitionKey,
                    Reference = documentId
                };
            }
            else
            {
                next = new ChainLink(last, documentId);
                last.Next = next.Id;
            }

            var tx = _container.CreateTransactionalBatch(new PartitionKey(_partitionKey));
            tx.CreateItem(next);

            if (last != null)
            {
                // Next got updated so update the previous item in the chain to point to the new one
                tx.ReplaceItem(last.Id.ToString(), last);
            }

            using var response = await tx.ExecuteAsync();

            // TODO(kevin): log status code

            return response.IsSuccessStatusCode;
        }

        public async Task<ChainLink> Tail()
        {
            var iterator = _container.GetItemLinqQueryable<ChainLink>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(_partitionKey)
            })
                .Where(x => x.Next.IsNull())
                .Take(1)
                .ToFeedIterator();

            return (await iterator.ReadNextAsync()).FirstOrDefault();
        }

        public sealed class ChainLink
        {
            public ChainLink(ChainLink previous, Guid reference)
            {
                Id = Guid.NewGuid();
                Previous = previous.Id;
                PartitionKey = previous.PartitionKey;
                Reference = reference;
            }

            public ChainLink() { }

            [JsonProperty("id")]
            public Guid Id { get; set; }

            // Reference to the previous link
            public Guid? Previous { get; set; }

            // Reference to the next link
            public Guid? Next { get; set; }

            // Reference to the document of this link
            public Guid Reference { get; set; }

            public string PartitionKey { get; set; }
        }
    }
}