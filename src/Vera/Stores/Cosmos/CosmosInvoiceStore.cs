using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using Vera.Audits;
using Vera.Models;

namespace Vera.Stores.Cosmos
{
    public sealed class CosmosInvoiceStore : IInvoiceStore
    {
        private readonly CosmosChain<Invoice> _chain;
        private readonly Container _container;

        public CosmosInvoiceStore(Container container)
        {
            _chain = new CosmosChain<Invoice>(container);
            _container = container;
        }

        public async Task Store(Invoice invoice, string bucket)
        {
            await _chain.Append(invoice, PartitionKeyByBucket(invoice.AccountId, bucket));

            // Also create document to look up by number efficiently
            var invoiceByNumberDocument = new Document<Invoice>(
                i => i.Id,
                i => PartitionKeyByNumber(i.AccountId, i.Number),
                invoice
            );

            // TODO(kevin): can store reference with id/partitionKey as well
            // but that would require 2 reads, not sure what's better here
            // also need to keep these documents in sync now
            await _container.CreateItemAsync(
                invoiceByNumberDocument,
                new PartitionKey(invoiceByNumberDocument.PartitionKey)
            );
        }

        public async Task<Invoice> Last(Guid accountId, string bucket)
        {
            var tail = await _chain.Tail(new PartitionKey(PartitionKeyByBucket(accountId, bucket)));

            return tail?.Value;
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

        private static string PartitionKeyByBucket(Guid accountId, string bucket) => $"{accountId}#B#{bucket}";
        private static string PartitionKeyByNumber(Guid accountId, string invoiceNumber) => $"{accountId}#N#{invoiceNumber}";
    }
}