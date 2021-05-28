using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Azure.Extensions;
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

        public Task<ICollection<EventLog>> List(EventLogCriteria eventLogCriteria)
        {
            var orderedQueryable = _container.GetItemLinqQueryable<Document<EventLog>>();

            var queryable = orderedQueryable
                .Where(x => x.Value.Supplier.AccountId == eventLogCriteria.AccountId &&
                            x.Value.Supplier.SystemId == eventLogCriteria.SupplierSystemId);

            if (eventLogCriteria.StartDate.HasValue)
            {
                queryable = queryable.Where(x => x.Value.Date >= eventLogCriteria.StartDate.Value);
            }

            if (eventLogCriteria.EndDate.HasValue)
            {
                queryable = queryable.Where(x => x.Value.Date <= eventLogCriteria.EndDate.Value);
            }

            if (eventLogCriteria.Type.HasValue)
            {
                queryable = queryable.Where(x => x.Value.Type == eventLogCriteria.Type.Value);
            }

            if (!string.IsNullOrEmpty(eventLogCriteria.RegisterId))
            {
                queryable = queryable.Where(x => x.Value.RegisterId == eventLogCriteria.RegisterId);
            }

            return queryable.ToListAsync();
        }
    }
}