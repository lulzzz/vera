using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using Vera.Models;

namespace Vera.Stores
{
    public sealed class CosmosStore : IInvoiceStore
    {
        private readonly string _connectionString;
        private readonly string _databaseId;
        private readonly string _containerId;

        public CosmosStore(string connectionString, string databaseId, string containerId)
        {
            _connectionString = connectionString;
            _databaseId = databaseId;
            _containerId = containerId;
        }

        public async Task Save(Invoice invoice, string bucket)
        {
            using var client = CreateClient();
            
            var container = client.GetContainer(_databaseId, _containerId);

            var partitionKey = GetPartitionKey(invoice);
            
            var document = new InvoiceDocument(invoice)
            {
                Bucket = bucket,
                PartitionKey = partitionKey
            };
            
            await container.CreateItemAsync(document, new PartitionKey(partitionKey));
        }

        public async Task<Invoice> Last(Invoice invoice, string bucket)
        {
            using var client = CreateClient();

            var container = client.GetContainer(_databaseId, _containerId);
            
            var iterator = container.GetItemLinqQueryable<InvoiceDocument>(requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(GetPartitionKey(invoice))
                })
                .Where(x => x.Bucket == bucket)
                .OrderByDescending(x => x.Timestamp)
                .Take(1)
                .ToFeedIterator();

            if (!iterator.HasMoreResults)
            {
                return null;
            }

            var document = (await iterator.ReadNextAsync()).FirstOrDefault();

            return document == null ? null : new Invoice(document);
        }

        private static string GetPartitionKey(Invoice invoice) =>
            $"{invoice.StoreNumber}-{invoice.FiscalPeriod}-{invoice.FiscalYear}";

        private CosmosClient CreateClient()
        {
            return new CosmosClientBuilder(_connectionString)
                .WithRequestTimeout(TimeSpan.FromSeconds(5))
                .WithConnectionModeDirect()
                .WithApplicationName("vera")
                .WithThrottlingRetryOptions(TimeSpan.FromSeconds(1), 5)
                .Build();
        }

        private class InvoiceDocument : Invoice
        {
            public InvoiceDocument(Invoice invoice)
            {
                Id = Guid.NewGuid();
                StoreNumber = invoice.StoreNumber;
                FiscalYear = invoice.FiscalYear;
                FiscalPeriod = invoice.FiscalPeriod;
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