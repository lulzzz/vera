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
                document => document.Supplier.Id.ToString(),
                eventLog
            );

            await _container.CreateItemAsync(byId);
        }

        public Task<ICollection<EventLog>> List(EventLogCriteria criteria)
        {
            var queryable = _container.GetItemLinqQueryable<Document<EventLog>>(requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(criteria.SupplierId.ToString())
                })
                .Where(x => x.Value.Supplier.AccountId == criteria.AccountId);

            if (criteria.StartDate.HasValue)
            {
                queryable = queryable.Where(x => x.Value.Date >= criteria.StartDate.Value);
            }

            if (criteria.EndDate.HasValue)
            {
                queryable = queryable.Where(x => x.Value.Date <= criteria.EndDate.Value);
            }

            if (criteria.Type.HasValue)
            {
                queryable = queryable.Where(x => x.Value.Type == criteria.Type.Value);
            }

            if (criteria.RegisterId.HasValue)
            {
                queryable = queryable.Where(x => x.Value.RegisterId == criteria.RegisterId);
            }

            return queryable.ToListAsync();
        }
    }
}
