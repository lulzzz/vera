using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Concurrency;
using Vera.Configuration;
using Vera.Portugal;
using Vera.Security;
using Vera.Services;
using Vera.Stores;

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

        private static void RegisterDefaults(IServiceCollection collection)
        {
            // Components
            collection.AddTransient<ITokenFactory, RandomTokenFactory>();
            collection.AddTransient<IPasswordStrategy, Pbkdf2PasswordStrategy>();
            collection.AddTransient<IAccountComponentFactoryCollection, AccountComponentFactoryCollection>();

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
                .Get<CosmosContainerOptions>();

            if (string.IsNullOrEmpty(cosmosOptions.ConnectionString) || string.IsNullOrEmpty(cosmosOptions.Database))
            {
                return;
            }

            var cosmosClient = new CosmosClientBuilder(cosmosOptions.ConnectionString)
                .WithRequestTimeout(TimeSpan.FromSeconds(5))
                .WithConnectionModeDirect()
                .WithApplicationName("vera")
                .WithThrottlingRetryOptions(TimeSpan.FromSeconds(1), 5)
                .Build();

            collection.AddSingleton(cosmosClient);

            collection.AddSingleton<IInvoiceStore>(_ => new CosmosInvoiceStore(
                cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Invoices)
            ));

            collection.AddTransient<ICompanyStore>(_ => new CosmosCompanyStore(
                cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Companies)
            ));

            collection.AddTransient<IAccountStore>(_ => new CosmosAccountStore(
                cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Companies)
            ));

            collection.AddTransient<IUserStore>(_ => new CosmosUserStore(
                cosmosClient.GetContainer(cosmosOptions.Database, cosmosContainerOptions.Users)
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