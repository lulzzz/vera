using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Vera.EventLogs;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public sealed class CosmosEventLogStore : IEventLogStore
    {
        private readonly Container _container;

        public CosmosEventLogStore(Container container)
        {
            _container = container;
        }

        public async Task Store(EventLog eventLog)
        {
            var byId = new Document<EventLog>(
                document => document.Id,
                document => document.Supplier.SystemId, 
                eventLog);

            await _container.CreateItemAsync(byId);
        }

        public async Task<ICollection<EventLog>> List(EventLogCriteria eventLogCriteria)
        {
            var query = new StringBuilder(@"
select value i
 from c[""Value""] i
where i.Date >= @startDate
  and i.Date <= @endDate
  and i.Supplier.AccountId = @accountId");

            QueryDefinition definition;
            if (eventLogCriteria.Type.HasValue)
            {
                query.Append("and i.Type = @type");

                definition = new QueryDefinition(query.ToString());
                definition.WithParameter("@type", eventLogCriteria.Type);
            }
            else
            {
                definition = new QueryDefinition(query.ToString());
            }

            definition
                .WithParameter("@startDate", eventLogCriteria.StartDate)
                .WithParameter("@endDate", eventLogCriteria.EndDate)
                .WithParameter("@accountId", eventLogCriteria.AccountId);

            var iterator = _container.GetItemQueryIterator<EventLog>(definition);

            var invoices = new List<EventLog>();

            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                invoices.AddRange(results);
            }

            return invoices;
        }
    }
}