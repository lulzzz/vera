using Microsoft.Azure.Cosmos;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        public async Task<Period> GetOpenPeriodForSupplier(Guid supplierId)
        {
            var definition = new QueryDefinition(@"
select top 1 value c[""Value""]
  from c
 where c.Type = @type
  and  c[""Value""].Supplier.Id = @supplierId
  and  c[""Value""].IsClosed = false")
              .WithParameter("@type", DocumentType)
              .WithParameter("@supplierId", supplierId.ToString());
            
            using var iterator = _container.GetItemQueryIterator<Period>(definition);

            var response = await iterator.ReadNextAsync();

            return response.FirstOrDefault();
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
                p => p.Supplier.Id.ToString(),
                period,
                DocumentType
            );
        }
    }
}
