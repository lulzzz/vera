using Microsoft.Azure.Cosmos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vera.Azure.Extensions;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public class CosmosSupplierStore : ISupplierStore
    {
        private const string DocumentType = "supplier";

        private readonly Container _container;

        public CosmosSupplierStore(Container container)
        {
            _container = container;
        }

        public async Task Store(Supplier supplier)
        {
            var document = ToDocument(supplier);

            await _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));
        }

        public Task<Supplier> Get(Guid accountId, string systemId)
        {
            var queryable = _container.GetItemLinqQueryable<TypedDocument<Supplier>>()
                .Where(x => x.Type == DocumentType
                && x.Value.SystemId == systemId
                && x.Value.AccountId == accountId);

            return queryable.FirstOrDefault();
        }


        public Task<Supplier> Get(Guid accountId, Guid supplierId)
        {
            var queryable = _container.GetItemLinqQueryable<TypedDocument<Supplier>>()
                .Where(x => x.Type == DocumentType
                && x.Value.Id == supplierId
                && x.Value.AccountId == accountId);

            return queryable.FirstOrDefault();
        }

        public Task Update(Supplier supplier)
        {
            var document = ToDocument(supplier);

            return _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey));
        }

        public Task Delete(Supplier supplier)
        {
            var document = ToDocument(supplier);

            return _container.DeleteItemAsync<TypedDocument<Supplier>>(
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey));
        }

        private static TypedDocument<Supplier> ToDocument(Supplier supplier)
        {
            return new(
                s => s.Id,
                s => s.AccountId.ToString(),
                supplier,
                DocumentType
            );
        }
    }
}
