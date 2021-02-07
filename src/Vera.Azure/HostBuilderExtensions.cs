using System;
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
using Vera.Stores.Cosmos;

namespace Vera.Azure
{
    public static class HostBuilderExtensions
    {
        private static volatile object _lock = new();
        
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

            // Locking here for the integration tests to prevent concurrency issues
            // when creating the database and containers. Integration test spins up
            // multiple instances of the app, every test get its own app that runs this
            // piece of code
            lock (_lock)
            {
                var response = client.CreateDatabaseIfNotExistsAsync(cosmosOptions.Database)
                    .GetAwaiter()
                    .GetResult();

                var db = response.Database;

                const string partitionKeyPath = "/PartitionKey";

                var containers = new[]
                {
                    cosmosContainerOptions.Companies,
                    cosmosContainerOptions.Invoices,
                    cosmosContainerOptions.Audits,
                    cosmosContainerOptions.Trails,
                    cosmosContainerOptions.Chains
                };

                foreach (var container in containers)
                {
                    // TODO(kevin): add unique key constraints
                    // TODO(kevin): add indexing policies
                    db.DefineContainer(container, partitionKeyPath)
                        .CreateIfNotExistsAsync()
                        .GetAwaiter()
                        .GetResult();
                }
            }

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

                if (string.IsNullOrEmpty(cosmosOptions.ConnectionString) || string.IsNullOrEmpty(cosmosOptions.Database))
                {
                    return;
                }

                var cosmosClient = new CosmosClientBuilder(cosmosOptions.ConnectionString)
                    // TODO(kevin): enable once this is supported in the SDK
                    // .WithContentResponseOnWrite(false)
                    .WithRequestTimeout(TimeSpan.FromSeconds(5))
                    .WithConnectionModeDirect()
                    .WithApplicationName("vera")
                    .WithThrottlingRetryOptions(TimeSpan.FromSeconds(1), 5)
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