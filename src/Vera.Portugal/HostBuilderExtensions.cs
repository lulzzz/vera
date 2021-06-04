using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Azure;
using Vera.Dependencies;
using Vera.Invoices;
using Vera.Portugal.Invoices;
using Vera.Portugal.Stores;

namespace Vera.Portugal
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseVeraPortugal(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, collection) =>
            {
                var cosmosOptions = context.Configuration
                    .GetSection(CosmosOptions.Section)
                    .Get<CosmosOptions>();

                var cosmosContainerOptions = context.Configuration
                    .GetSection(CosmosContainerOptions.Section)
                    .Get<CosmosContainerOptions>() ?? new CosmosContainerOptions();

                collection.AddSingleton<IWorkingDocumentStore, CosmosWorkingDocumentStore>(sp =>
                {
                    var client = sp.GetRequiredService<CosmosClient>();
                    var container = client.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Documents);

                    return new CosmosWorkingDocumentStore(container);
                });

                collection.AddSingleton<IAccountComponentFactory, AccountComponentFactory>();
                collection.AddTransient<IInvoiceHandlerFactory, PortugalInvoiceHandlerFactory>();
            });

            return builder;
        }
    }
}
