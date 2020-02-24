using System;
using System.Diagnostics;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Vera.Stores;

namespace Vera
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
            });

            return builder;
        }
    }
}