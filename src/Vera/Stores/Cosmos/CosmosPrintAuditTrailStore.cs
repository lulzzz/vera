using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Vera.Models;

namespace Vera.Stores.Cosmos
{
    public class CosmosPrintAuditTrailStore : IPrintAuditTrailStore
    {
        private readonly Container _container;

        public CosmosPrintAuditTrailStore(Container container)
        {
            _container = container;
        }

        public async Task<PrintTrail> Create(Guid invoiceId)
        {
            var trail = new PrintTrail
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow,
                InvoiceId = invoiceId
            };

            var document = ToDocument(trail);

            await _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));

            return trail;
        }

        public Task Update(PrintTrail trail)
        {
            return _container.ReplaceItemAsync(
                trail,
                trail.Id.ToString(),
                new PartitionKey(trail.InvoiceId.ToString())
            );
        }

        public async Task<PrintTrail> Get(Guid trailId, Guid invoiceId)
        {
            var response = await _container.ReadItemAsync<Document<PrintTrail>>(
                trailId.ToString(),
                new PartitionKey(invoiceId.ToString())
            );

            return response.Resource.Value;
        }

        public async Task<ICollection<PrintTrail>> GetByInvoice(Guid invoiceId)
        {
            var query = new QueryDefinition(@"select value c[""Value""] from c");
            using var iterator = _container.GetItemQueryIterator<PrintTrail>(query, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(invoiceId.ToString())
            });

            var results = new List<PrintTrail>();

            while (iterator.HasMoreResults)
            {
                results.AddRange(await iterator.ReadNextAsync());
            }

            return results;
        }

        private static Document<PrintTrail> ToDocument(PrintTrail trail)
        {
            return new(
                t => t.Id,
                t => t.InvoiceId.ToString(),
                trail
            );
        }
    }
}