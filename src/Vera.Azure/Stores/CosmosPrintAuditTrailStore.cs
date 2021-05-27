using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Azure.Extensions;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
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
            var document = ToDocument(trail);

            return _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey)
            );
        }

        public async Task<PrintTrail> Get(Guid invoiceId, Guid trailId)
        {
            var response = await _container.ReadItemAsync<Document<PrintTrail>>(
                trailId.ToString(),
                new PartitionKey(invoiceId.ToString())
            );

            return response.Resource.Value;
        }

        public Task<ICollection<PrintTrail>> GetByInvoice(Guid invoiceId)
        {
            var queryable = _container.GetItemLinqQueryable<Document<PrintTrail>>()
                .Where(x => x.Value.InvoiceId == invoiceId);

            return queryable.ToListAsync();
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