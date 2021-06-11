using Microsoft.Azure.Cosmos;
using System;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public class CosmosGrandTotalAuditTrailStore : IGrandTotalAuditTrailStore
    {
        private readonly Container _container;

        public CosmosGrandTotalAuditTrailStore(Container container)
        {
            _container = container;
        }

        public async Task<GrandTotalAuditTrail> Create(Invoice invoice, decimal grandTotal)
        {
            var trail = new GrandTotalAuditTrail
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                SupplierId = invoice.Supplier.Id,
                GrandTotal = grandTotal + invoice.Totals.Gross
            };

            var document = ToDocument(trail);

            await _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));

            return trail;
        }

        public async Task Delete(GrandTotalAuditTrail grandTotalAuditTrail)
        {
            var response = await _container.DeleteItemStreamAsync(
                grandTotalAuditTrail.Id.ToString(),
                new PartitionKey(grandTotalAuditTrail.SupplierId.ToString())
            );

            response.EnsureSuccessStatusCode();
        }

        private static Document<GrandTotalAuditTrail> ToDocument(GrandTotalAuditTrail trail)
        {
            return new(
                t => t.Id,
                t => t.SupplierId.ToString(),
                trail
            );
        }
    }
}
