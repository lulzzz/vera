using Microsoft.Azure.Cosmos;
using System;
using System.Net;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public class CosmosReportStore : IReportStore
    {
        private const string DocumentType = "report";

        private readonly Container _container;

        public CosmosReportStore(Container container)
        {
            _container = container;
        }

        public async Task Store(RegisterReport report)
        {
            var document = ToDocument(report);

            await _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));
        }

        public async Task<RegisterReport> Get(Guid reportId)
        {
            try
            {
                var document = await _container.ReadItemAsync<TypedDocument<RegisterReport>>(
                    reportId.ToString(),
                    new PartitionKey(reportId.ToString())
                );

                return document.Resource.Value;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public Task Delete(RegisterReport registerReport)
        {
            var document = ToDocument(registerReport);

            return _container.DeleteItemAsync<TypedDocument<RegisterReport>>(
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey));
        }

        private static TypedDocument<RegisterReport> ToDocument(RegisterReport report)
        {
            return new(
                s => s.Id,
                s => s.Id.ToString(),
                report,
                DocumentType
            );
        }

    }
}
