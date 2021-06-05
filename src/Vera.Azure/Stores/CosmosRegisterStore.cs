using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Linq;
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

        public async Task<int> GetTotalRegisters(Guid supplierId)
        {
            var totalRegisters = await _container.GetItemLinqQueryable<Document<Register>>(
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(supplierId.ToString())
                    })
                .Where(x => x.Value.SupplierId == supplierId)
                .CountAsync();

            return totalRegisters;
        }

        public async Task<Register> Get(Guid supplierId, Guid registerId)
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

        public Task<Register> GetBySystemIdAndSupplierId(Guid supplierId, string systemId)
        {
            try
            {
                var queryable = _container.GetItemLinqQueryable<Document<Register>>(requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(supplierId.ToString())
                    })
                    .Where(x => x.Value.SystemId == systemId);

                return queryable.FirstOrDefault();
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public Task<ICollection<Register>> GetOpenRegistersForSupplier(Guid supplierId)
        {
            var queryable = _container.GetItemLinqQueryable<Document<Register>>(requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(supplierId.ToString())
                })
                .Where(x => x.Value.Status == RegisterStatus.Open);

            return queryable.ToListAsync();
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
