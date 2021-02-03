using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Concurrency;
using Vera.Portugal;
using Vera.Security;
using Vera.Services;
using Vera.Stores;
using Vera.Stores.Cosmos;

namespace Vera.Bootstrap
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseVera(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, collection) =>
            {
                RegisterDefaults(collection);

                // TODO(kevin): extract so other methods can be used
                UseCosmosStores(context, collection);
                UseBlobLocker(context, collection);
            });

            builder.UseVeraPortugal();

            return builder;
        }

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

            var response = client.CreateDatabaseIfNotExistsAsync(cosmosOptions.Database).GetAwaiter().GetResult();
            var db = response.Database;

            const string partitionKeyPath = "/PartitionKey";

            var containers = new[]
            {
                cosmosContainerOptions.Companies,
                cosmosContainerOptions.Users,
                cosmosContainerOptions.Invoices,
                cosmosContainerOptions.Audits
            };

            foreach (var container in containers)
            {
                db.CreateContainerIfNotExistsAsync(container, partitionKeyPath).GetAwaiter().GetResult();
            }

            return host;
        }

        private static void RegisterDefaults(IServiceCollection collection)
        {
            // Components
            collection.AddTransient<ITokenFactory, RandomTokenFactory>();
            collection.AddTransient<IPasswordStrategy, Pbkdf2PasswordStrategy>();
            collection.AddTransient<IAccountComponentFactoryCollection, AccountComponentFactoryCollection>();

            // Stores
            collection.AddTransient<IBlobStore, TemporaryBlobStore>();

            // Services
            collection.AddTransient<IUserRegisterService, UserRegisterService>();
        }

        private static void UseCosmosStores(HostBuilderContext context, IServiceCollection collection)
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
                cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Invoices)
            ));

            collection.AddSingleton<ICompanyStore>(_ => new CosmosCompanyStore(
                cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Companies)
            ));

            collection.AddSingleton<IAccountStore>(_ => new CosmosAccountStore(
                cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Companies)
            ));

            collection.AddSingleton<IUserStore>(_ => new CosmosUserStore(
                cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Users)
            ));

            collection.AddSingleton<IAuditStore>(_ => new CosmosAuditStore(
                cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Audits)
            ));
        }

        private static void UseBlobLocker(HostBuilderContext context, IServiceCollection collection)
        {
            var blobConnectionString = context.Configuration["VERA:BLOB:CONNECTIONSTRING"];

            if (!string.IsNullOrEmpty(blobConnectionString))
            {
                collection.AddSingleton<ILocker>(new AzureBlobLocker(blobConnectionString));
            }
        }
    }
}