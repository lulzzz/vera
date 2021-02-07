using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Vera.Audits;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public class CosmosAuditStore : IAuditStore
    {
        private readonly Container _container;

        public CosmosAuditStore(Container container)
        {
            _container = container;
        }

        public async Task<Audit> Create(AuditCriteria criteria)
        {
            var audit = new Audit
            {
                Id = Guid.NewGuid(),
                AccountId = criteria.AccountId,
                SupplierSystemId = criteria.SupplierSystemId,
                Date = DateTime.UtcNow,
                Start = criteria.StartDate,
                End = criteria.EndDate
            };

            var document = ToDocument(audit);

            await _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));

            return audit;
        }

        public async Task<Audit> Get(Guid accountId, Guid auditId)
        {
            var document = await _container.ReadItemAsync<Document<Audit>>(
                auditId.ToString(),
                new PartitionKey(accountId.ToString())
            );

            return document.Resource.Value;
        }

        public Task Update(Audit audit)
        {
            var document = ToDocument(audit);

            return _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey)
            );
        }

        private static Document<Audit> ToDocument(Audit audit)
        {
            return new(
                a => a.Id,
                a => a.AccountId.ToString(),
                audit
            );
        }
    }
}