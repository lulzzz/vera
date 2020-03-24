using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using Vera.Audit;
using Vera.Models;

namespace Vera.Stores
{
    public sealed class CosmosInvoiceStore : IInvoiceStore
    {
        private readonly Container _container;

        public CosmosInvoiceStore(Container container)
        {
            _container = container;
        }

        public async Task Save(Invoice invoice, string bucket)
        {
            var chain = new CosmosChain(_container, bucket);

            var partitionKey = GetPartitionKey(invoice);

            var document = new InvoiceDocument(invoice)
            {
                Bucket = bucket,
                PartitionKey = partitionKey
            };

            try
            {
                // Try to insert the invoice first, if that fails then the chain need never be created
                await _container.CreateItemAsync(document, new PartitionKey(partitionKey));

                await chain.Append(document.Id);
            }
            catch (CosmosException)
            {
                // TODO(kevin): check what exception happened
                using var response = await _container.DeleteItemStreamAsync(
                    document.Id.ToString(),
                    new PartitionKey(partitionKey)
                );

                throw;
            }
        }

        public async Task<Invoice> Last(Invoice invoice, string bucket)
        {
            var chain = new CosmosChain(_container, bucket);

            var tail = await chain.Tail();

            if (tail == null)
            {
                return null;
            }

            var last = await _container.ReadItemAsync<InvoiceDocument>(
                tail.Reference.ToString(),
                new PartitionKey(GetPartitionKey(invoice))
            );

            return last?.Resource.Invoice;
        }
        
        public async IAsyncEnumerable<Invoice> List(AuditCriteria criteria)
        {
            // TODO(kevin): paging
            var iterator = _container.GetItemLinqQueryable<InvoiceDocument>()
                .Where(x => x.Invoice.Supplier.SystemID == criteria.SupplierSystemId &&
                            x.Invoice.FiscalYear >= criteria.StartFiscalYear && 
                            x.Invoice.FiscalYear <= criteria.EndFiscalYear &&
                            x.Invoice.FiscalPeriod >= criteria.StartFiscalPeriod && 
                            x.Invoice.FiscalPeriod <= criteria.EndFiscalPeriod
                )
                .ToFeedIterator();

            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();

                foreach (var result in results)
                {
                    yield return result.Invoice;
                }
            }
        }

        private static string GetPartitionKey(Invoice invoice) =>
            $"{invoice.Supplier.SystemID}-{invoice.FiscalYear}-{invoice.FiscalPeriod}";

        private class InvoiceDocument
        {
            public InvoiceDocument(Invoice invoice)
            {
                Id = Guid.NewGuid();
                Invoice = new Invoice(invoice);
            }

            public InvoiceDocument() { }

            [JsonProperty("id")]
            public Guid Id { get; set; }

            public Invoice Invoice { get; set; }

            public string Bucket { get; set; }

            public string PartitionKey { get; set; }
        }
    }
}