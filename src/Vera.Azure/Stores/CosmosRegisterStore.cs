using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vera.Azure.Extensions;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public class CosmosRegisterStore : IRegisterStore
    {
        private readonly Container _container;

        public CosmosRegisterStore(Container container)
        {
            _container = container;
        }

        public async Task<Register> Get(Guid registerId, Guid supplierId)
        {
            try
            {
                var document = await _container.ReadItemAsync<Document<Register>>(
                    registerId.ToString(),
                    new PartitionKey(supplierId.ToString())
                );

                return document.Resource.Value;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Register> GetBySystemIdAndSupplierId(string systemId, Guid supplierId)
        {
            try
            {
                return await _container.GetItemLinqQueryable<Document<Register>>(true)
                    .Where(x => x.Value.SystemId == systemId &&
                                x.Value.SupplierId == supplierId)
                    .FirstOrDefault();
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<ICollection<Register>> GetOpenRegistersForSupplier(Guid supplierId)
        {
            var queryable = _container.GetItemLinqQueryable<Document<Register>>(true)
                .Where(x => x.Value.Status == RegisterStatus.Open && x.Value.SupplierId == supplierId);

            return await queryable.ToListAsync();
        }

        public async Task<ICollection<Register>> GetRegistersBasedOnSupplier(IEnumerable<Guid> registersIds, Guid supplierId)
        {
            var query = new StringBuilder(@"
                SELECT value c[""Value""] 
                FROM c 
                WHERE 
                    c[""Value""].SupplierId = @supplierId AND
                    ARRAY_CONTAINS(@registersIds, c[""Value""].Id)");

            var definition = new QueryDefinition(query.ToString());
            definition.WithParameter("@supplierId", supplierId);
            definition.WithParameter("@registersIds", registersIds);

            var iterator = _container.GetItemQueryIterator<Register>(definition);

            var regiters = new List<Register>();

            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                regiters.AddRange(results);
            }

            return regiters;
        }

        public async Task Store(Register register)
        {
            var document = ToDocument(register);

            await _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));
        }

        public Task Update(Register register)
        {
            var document = ToDocument(register);

            return _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey));
        }

        private static Document<Register> ToDocument(Register register)
        {
            return new(
                r => r.Id,
                r => r.SupplierId.ToString(),
                register
            );
        }
    }
}
