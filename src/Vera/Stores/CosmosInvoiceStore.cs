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
        private readonly CosmosChain<Invoice> _chain;
        private readonly Container _container;

        public CosmosInvoiceStore(Container container)
        {
            _chain = new CosmosChain<Invoice>(container);
            _container = container;
        }

        public async Task Save(Invoice invoice, string bucket)
        {
            await _chain.Append(invoice, bucket);
        }

        public async Task<Invoice> Last(string bucket)
        {
            var tail = await _chain.Tail(new PartitionKey(bucket));

            return tail?.Value;
        }

        public async Task<Invoice> GetByNumber(Guid accountId, string number)
        {
            var definition = new QueryDefinition("select * from c");

            using var iterator = _container.GetItemQueryIterator<InvoiceDocument>(definition, requestOptions: new QueryRequestOptions
            {
                // TODO(kevin): partition key creation is duplicated in change feed processor
                PartitionKey = new PartitionKey(accountId + "-" + number)
            });

            var response = await iterator.ReadNextAsync();

            return response.FirstOrDefault()?.Invoice;
        }

        public async IAsyncEnumerable<Invoice> List(AuditCriteria criteria)
        {
            // TODO(kevin): paging
            var query = _chain.Query().Where(x => x.Value.AccountId == criteria.AccountId);

            if (!string.IsNullOrWhiteSpace(criteria.SupplierSystemId))
            {
                query = query.Where(x => x.Value.Supplier.SystemId == criteria.SupplierSystemId);
            }

            query = query.Where(x =>
                x.Value.FiscalYear >= criteria.StartFiscalYear &&
                x.Value.FiscalYear <= criteria.EndFiscalYear &&
                x.Value.FiscalPeriod >= criteria.StartFiscalPeriod &&
                x.Value.FiscalPeriod <= criteria.EndFiscalPeriod
            );

            var iterator = query.ToFeedIterator();
            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();

                foreach (var result in results)
                {
                    yield return result.Value;
                }
            }
        }

        public class InvoiceDocument
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            public Invoice Invoice { get; set; }
            public string PartitionKey { get; set; }
        }
    }
}