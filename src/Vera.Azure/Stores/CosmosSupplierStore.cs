using Microsoft.Azure.Cosmos;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        public async Task<Supplier> Get(Guid supplierId)
        {
            try
            {
                var document = await _container.ReadItemAsync<TypedDocument<Supplier>>(
                    supplierId.ToString(),
                    new PartitionKey(supplierId.ToString())
                );

                return document.Resource.Value;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Supplier> GetBySystemId(string systemId)
        {
            var definition = new QueryDefinition(@"
select top 1 value c[""Value""]
  from c
 where c.Type = @type
  and  c[""Value""].SystemId = @systemId")
                .WithParameter("@type", DocumentType)
                .WithParameter("@systemId", systemId);

            using var iterator = _container.GetItemQueryIterator<Supplier>(definition);

            var response = await iterator.ReadNextAsync();

            return response.FirstOrDefault();
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
                s => s.Id.ToString(),
                supplier,
                DocumentType
            );
        }
    }
}
