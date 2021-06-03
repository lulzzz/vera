using Microsoft.Azure.Cosmos;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Vera.Azure.Extensions;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public class CosmosPeriodStore : IPeriodStore
    {
        private const string DocumentType = "period";

        private readonly Container _container;

        public CosmosPeriodStore(Container container)
        {
            _container = container;
        }

        public async Task Store(Period period)
        {
            var document = ToDocument(period);

            await _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));
        }

        public async Task<Period> Get(Guid periodId, Guid supplierId)
        {
            try
            {
                var document = await _container.ReadItemAsync<TypedDocument<Period>>(
                    periodId.ToString(),
                    new PartitionKey(supplierId.ToString())
                );

                return document.Resource.Value;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }


        public Task<Period> GetOpenPeriodForSupplier(Guid supplierId)
        {
            var queryable = _container.GetItemLinqQueryable<TypedDocument<Period>>(requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(supplierId.ToString()),
                    MaxItemCount = 1
                })
                .Where(x => x.Type == DocumentType && !x.Value.IsClosed);

            return queryable.FirstOrDefault();
        }

        public Task Update(Period period)
        {
            var document = ToDocument(period);

            return _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey));
        }

        private static TypedDocument<Period> ToDocument(Period period)
        {
            return new(
                p => p.Id,
                p => p.SupplierId.ToString(),
                period,
                DocumentType
            );
        }
    }
}
