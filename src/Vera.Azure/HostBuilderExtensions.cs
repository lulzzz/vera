using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Serilog;
using Vera.Azure.Concurrency;
using Vera.Azure.Stores;
using Vera.Concurrency;
using Vera.Stores;

namespace Vera.Azure
{
    public static class HostBuilderExtensions
    {
        private static readonly Mutex Mutex = new();
        private static volatile bool _created;

        // TODO(kevin): do not like this, but also don't know a better way for this do-once on startup kind of work
        public static IHost ConfigureCosmos(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var cosmosOptions = config
                .GetSection(CosmosOptions.Section)
                .Get<CosmosOptions>();

            var cosmosContainerOptions = config
                .GetSection(CosmosContainerOptions.Section)
                .Get<CosmosContainerOptions>() ?? new CosmosContainerOptions();

            var client = scope.ServiceProvider.GetRequiredService<CosmosClient>();

            if (_created)
            {
                return host;
            }
            
            // Locking here for the integration tests to prevent concurrency issues
            // when creating the database and containers. Integration test spins up
            // multiple instances of the app, every test get its own app that runs this
            // piece of code
            Mutex.WaitOne();

            if (_created)
            {
                Mutex.ReleaseMutex();
                return host;
            }

            var throughput = ThroughputProperties.CreateManualThroughput(400);

            var response = client.CreateDatabaseIfNotExistsAsync(cosmosOptions.Database, throughput)
                .GetAwaiter()
                .GetResult();

            const string partitionKeyPath = "/PartitionKey";

            var containers = new[]
            {
                cosmosContainerOptions.Companies,
                cosmosContainerOptions.Invoices,
                cosmosContainerOptions.Audits,
                cosmosContainerOptions.Trails,
                cosmosContainerOptions.Chains
            };

            var db = response.Database;

            foreach (var container in containers)
            {
                db
                    .DefineContainer(container, partitionKeyPath)
                    .CreateIfNotExistsAsync()
                    .GetAwaiter()
                    .GetResult();
            }

            _created = true;

            Mutex.ReleaseMutex();

            return host;
        }

        public static IHostBuilder UseAzure(this IHostBuilder builder)
        {
            return builder
                .UseCosmosStores()
                .UseAzureBlobs();
        }

        /// <summary>
        /// Use Cosmos as the main backing store for the entities.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <seealso cref="CosmosOptions"/>
        /// <seealso cref="CosmosContainerOptions"/>
        public static IHostBuilder UseCosmosStores(this IHostBuilder builder)
        {
            return builder.ConfigureServices((context, collection) =>
            {
                var cosmosOptions = context.Configuration
                    .GetSection(CosmosOptions.Section)
                    .Get<CosmosOptions>();

                var cosmosContainerOptions = context.Configuration
                    .GetSection(CosmosContainerOptions.Section)
                    .Get<CosmosContainerOptions>() ?? new CosmosContainerOptions();

                if (string.IsNullOrEmpty(cosmosOptions.ConnectionString))
                {
                    Log.Error("cannot register cosmos stores because connection string is missing");
                    return;
                }

                if (string.IsNullOrEmpty(cosmosOptions.Database))
                {
                    Log.Error("cannot register cosmos stores because database is missing");
                    return;
                }

                var cosmosClient = new CosmosClientBuilder(cosmosOptions.ConnectionString)
                    .WithContentResponseOnWrite(false)
                    .WithConnectionModeDirect()
                    .WithRequestTimeout(TimeSpan.FromSeconds(5))
                    .WithThrottlingRetryOptions(TimeSpan.FromSeconds(1), 5)
                    .WithApplicationName("vera")
                    .Build();

                collection.AddSingleton(cosmosClient);

                collection.AddSingleton<IInvoiceStore>(_ => new CosmosInvoiceStore(
                    cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Invoices)));

                collection.AddSingleton<ICompanyStore>(_ => new CosmosCompanyStore(
                    cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Companies)));

                collection.AddSingleton<IAccountStore>(_ => new CosmosAccountStore(
                    cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Companies)));

                collection.AddSingleton<IUserStore>(_ => new CosmosUserStore(
                    cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Companies)));

                collection.AddSingleton<IAuditStore>(_ => new CosmosAuditStore(
                    cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Audits)));

                collection.AddSingleton<IPrintAuditTrailStore>(_ => new CosmosPrintAuditTrailStore(
                    cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Trails)));

                collection.AddSingleton<IChainStore>(_ => new CosmosChainStore(
                    cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Chains)));
            });
        }

        public static IHostBuilder UseAzureBlobs(this IHostBuilder builder)
        {
            return builder.ConfigureServices((context, collection) =>
            {
                // Do not configure Azure dependency when running in development mode
                if (context.HostingEnvironment.IsDevelopment())
                {
                    Log.Warning("not using Azure blob storage because we're running in development mode");
                    return;
                }

                var blobConnectionString = context.Configuration["VERA:BLOB:CONNECTIONSTRING"];

                if (string.IsNullOrEmpty(blobConnectionString))
                {
                    Log.Warning("not using Azure blob storage because the connection string is empty");
                    return;
                }

                collection.Replace(ServiceDescriptor.Singleton<ILocker>(new AzureBlobLocker(blobConnectionString)));
                collection.Replace(ServiceDescriptor.Singleton<IBlobStore>(new AzureBlobStore(blobConnectionString)));
            });
        }
    }
}