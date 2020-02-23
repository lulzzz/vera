using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Newtonsoft.Json;
using Vera.Models;

namespace Vera.Stores
{
    public sealed class CosmosStore : IInvoiceStore
    {
        private readonly CosmosClient _client;
        private readonly string _databaseId;
        private readonly string _containerId;

        public CosmosStore(string connectionString, string databaseId, string containerId)
        {
            _client = CreateClient(connectionString);
            _databaseId = databaseId;
            _containerId = containerId;
        }

        public async Task Save(Invoice invoice, string bucket)
        {
            var container = _client.GetContainer(_databaseId, _containerId);
            var chain = new CosmosChain(container, bucket);

            var partitionKey = GetPartitionKey(invoice);

            var document = new InvoiceDocument(invoice)
            {
                Bucket = bucket,
                PartitionKey = partitionKey
            };

            // TODO(kevin): want to make sure that the creation and appending to the chain both succeed
            // otherwise a rollback is required, somehow, by deleting the document(s)
            await chain.Append(document.Id);

            // TODO(kevin): try/catch and rollback chain
            await container.CreateItemAsync(document, new PartitionKey(partitionKey));
        }

        public async Task<Invoice> Last(Invoice invoice, string bucket)
        {
            var container = _client.GetContainer(_databaseId, _containerId);
            var chain = new CosmosChain(container, bucket);

            var tail = await chain.Tail();

            if (tail == null)
            {
                return null;
            }

            var last = await container.ReadItemAsync<InvoiceDocument>(
                tail.Reference.ToString(),
                new PartitionKey(GetPartitionKey(invoice))
            );

            if (last == null)
            {
                return null;
            }

            return new Invoice(last);
        }

        private static string GetPartitionKey(Invoice invoice) =>
            $"{invoice.StoreNumber}-{invoice.FiscalPeriod}-{invoice.FiscalYear}";

        private CosmosClient CreateClient(string connectionString)
        {
            return new CosmosClientBuilder(connectionString)
                .WithRequestTimeout(TimeSpan.FromSeconds(5))
                .WithConnectionModeDirect()
                .WithApplicationName("vera")
                .WithThrottlingRetryOptions(TimeSpan.FromSeconds(1), 5)
                .Build();
        }

        private class InvoiceDocument : Invoice
        {
            public InvoiceDocument(Invoice invoice) : base(invoice)
            {
                Id = Guid.NewGuid();
            }

            public InvoiceDocument() { }

            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("_ts")]
            public long Timestamp { get; set; }

            public string Bucket { get; set; }

            public string PartitionKey { get; set; }
        }
    }
}