using Microsoft.Azure.Cosmos;
using System;
using System.Linq;
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

        public async Task<RegisterReport> GetByNumber(Guid accountId, string number)
        {
            try
            {
                var definition = new QueryDefinition(@"select value c[""Value""] from c");

                using var iterator = _container.GetItemQueryIterator<RegisterReport>(definition,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(PartitionKeyByNumber(accountId, number)),
                        MaxItemCount = 1
                    });

                var response = await iterator.ReadNextAsync();

                return response.FirstOrDefault();
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public Task Delete(RegisterReport registerReport)
        {
            var document = ToDocument(registerReport);

            return _container.DeleteItemStreamAsync(
                document.Id.ToString(),
                new PartitionKey(PartitionKeyByNumber(registerReport.Account.Id, registerReport.Number)));
        }

        private static TypedDocument<RegisterReport> ToDocument(RegisterReport report)
        {
            var accountId = report.Account.Id;
            return new TypedDocument<RegisterReport>(
                s => s.Id,
                s => PartitionKeyByNumber(accountId, s.Number),
                report,
                DocumentType
            );
        }

        private static string PartitionKeyByNumber(Guid accountId,
            string reportNumber) => $"{accountId}#N#{reportNumber}";
    }
}
