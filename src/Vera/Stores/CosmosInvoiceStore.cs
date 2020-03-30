using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Vera.Audit;
using Vera.Models;

namespace Vera.Stores
{
    public sealed class CosmosInvoiceStore : IInvoiceStore
    {
        private readonly CosmosChain<Invoice> _chain;

        public CosmosInvoiceStore(Container container)
        {
            _chain = new CosmosChain<Invoice>(container);
        }

        public async Task Save(Invoice invoice, string bucket)
        {
            await _chain.Append(invoice, new PartitionKey(bucket));
        }

        public async Task<Invoice> Last(Invoice invoice, string bucket)
        {
            var tail = await _chain.Tail(new PartitionKey(bucket));

            return tail?.Value;
        }
        
        public async IAsyncEnumerable<Invoice> List(AuditCriteria criteria)
        {
            // TODO(kevin): paging
            var query = _chain.Query().Where(x => x.Value.AccountId == criteria.AccountId);

            if (!string.IsNullOrWhiteSpace(criteria.SupplierSystemId))
            {
                query = query.Where(x => x.Value.Supplier.SystemID == criteria.SupplierSystemId);
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
    }
}