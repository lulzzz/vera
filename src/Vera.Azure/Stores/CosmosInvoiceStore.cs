using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Vera.Audits;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public sealed class CosmosInvoiceStore : IInvoiceStore
    {
        private readonly Container _container;

        public CosmosInvoiceStore(Container container)
        {
            _container = container;
        }

        public async Task Store(Invoice invoice)
        {
            var byNumber = new Document<Invoice>(
                i => i.Id,
                i => PartitionKeyByNumber(i.AccountId, i.Number),
                invoice
            );
            
            await _container.CreateItemAsync(byNumber, new PartitionKey(byNumber.PartitionKey));
        }

        public async Task<Invoice> GetByNumber(Guid accountId, string number)
        {
            var definition = new QueryDefinition(@"select value c[""Value""] from c");

            using var iterator = _container.GetItemQueryIterator<Invoice>(definition, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(PartitionKeyByNumber(accountId, number))
            });

            var response = await iterator.ReadNextAsync();

            return response.FirstOrDefault();
        }

        public async Task<ICollection<Invoice>> List(AuditCriteria criteria)
        {
            // TODO(kevin): filter on fiscal period(s) instead of start/end date
            var definition = new QueryDefinition(@"
select value i
 from c[""Value""] i
where i.AccountId = @accountId
  and i.Date >= @startDate
  and i.Date <= @endDate
");

            definition
                .WithParameter("@accountId", criteria.AccountId)
                .WithParameter("@startDate", criteria.StartDate)
                .WithParameter("@endDate", criteria.EndDate);

            var iterator = _container.GetItemQueryIterator<Invoice>(definition);

            var invoices = new List<Invoice>();

            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                invoices.AddRange(results);
            }

            return invoices;
        }

        public async Task Delete(Invoice invoice)
        {
            var response = await _container.DeleteItemStreamAsync(
                invoice.Id.ToString(),
                new PartitionKey(PartitionKeyByNumber(invoice.AccountId, invoice.Number))
            );

            response.EnsureSuccessStatusCode();
        }

        private static string PartitionKeyByBucket(Guid accountId, string bucket) => $"{accountId}#B#{bucket}";
        private static string PartitionKeyByNumber(Guid accountId, string invoiceNumber) => $"{accountId}#N#{invoiceNumber}";
    }
}