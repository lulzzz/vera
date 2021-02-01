using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Vera.Audits;
using Vera.Models;

namespace Vera.Stores.Cosmos
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

            var document = new AuditDocument(audit);

            await _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));

            return audit;
        }

        public async Task<Audit> Get(Guid accountId, Guid auditId)
        {
            var document = await _container.ReadItemAsync<AuditDocument>(
                auditId.ToString(),
                new PartitionKey(accountId.ToString())
            );

            return document.Resource?.Audit;
        }

        public Task Update(Audit audit)
        {
            var document = new AuditDocument(audit);

            // TODO(kevin): ensure that just properties that we deem mutable are persisted
            // TODO(kevin): ^ pretty much just the Location property at the moment

            return _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey)
            );
        }

        private class AuditDocument
        {
            public AuditDocument() { }

            public AuditDocument(Audit audit)
            {
                Id = audit.Id;
                Audit = audit;
                PartitionKey = audit.AccountId.ToString();
            }

            [JsonProperty("id")]
            public Guid Id { get; set; }

            public Audit Audit { get; set; }

            public string PartitionKey { get; set; }
        }
    }
}