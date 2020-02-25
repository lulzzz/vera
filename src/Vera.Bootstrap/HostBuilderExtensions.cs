using System;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Concurrency;
using Vera.Portugal;
using Vera.Stores;

namespace Vera.Bootstrap
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseVera(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, collection) =>
            {
                var cosmosConnectionString = context.Configuration["VERA:COSMOS:CONNECTIONSTRING"];
                var cosmosDatabase = context.Configuration["VERA:COSMOS:DATABASE"];
                var cosmosContainer = context.Configuration["VERA:COSMOS:CONTAINER"];

                if (!string.IsNullOrEmpty(cosmosConnectionString) &&
                    !string.IsNullOrEmpty(cosmosDatabase) &&
                    !string.IsNullOrEmpty(cosmosContainer))
                {
                    collection.AddSingleton<IInvoiceStore>(provider =>
                    {
                        var cosmosClient = new CosmosClientBuilder(cosmosConnectionString)
                            .WithRequestTimeout(TimeSpan.FromSeconds(5))
                            .WithConnectionModeDirect()
                            .WithApplicationName("vera")
                            .WithThrottlingRetryOptions(TimeSpan.FromSeconds(1), 5)
                            .Build();

                        return new CosmosInvoiceStore(cosmosClient, cosmosDatabase, cosmosContainer);
                    });
                }

                var blobConnectionString = context.Configuration["VERA:BLOB:CONNECTIONSTRING"];

                if (!string.IsNullOrEmpty(blobConnectionString))
                {
                    collection.AddSingleton<ILocker>(new AzureBlobLocker(blobConnectionString));
                }

                collection.AddSingleton<IComponentFactoryCollection, ComponentFactoryCollection>();
            });

            builder.UseVeraPortugal();

            return builder;
        }
    }
}