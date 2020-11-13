using System;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Concurrency;
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
            collection.AddTransient<IComponentFactoryCollection, ComponentFactoryCollection>();

            // Facades
            collection.AddTransient<IUserRegisterService, UserRegisterService>();
        }

        private static void UseCosmosStores(HostBuilderContext context, IServiceCollection collection)
        {
            var cosmosConnectionString = context.Configuration["VERA:COSMOS:CONNECTIONSTRING"];
            var cosmosDatabase = context.Configuration["VERA:COSMOS:DATABASE"];

            if (string.IsNullOrEmpty(cosmosConnectionString) || string.IsNullOrEmpty(cosmosDatabase))
            {
                return;
            }

            var cosmosContainerInvoices = context.Configuration["VERA:COSMOS:CONTAINER:INVOICES"];
            var cosmosContainerCompanies = context.Configuration["VERA:COSMOS:CONTAINER:COMPANIES"];
            var cosmosContainerUsers = context.Configuration["VERA:COSMOS:CONTAINER:USERS"];

            var cosmosClient = new CosmosClientBuilder(cosmosConnectionString)
                .WithRequestTimeout(TimeSpan.FromSeconds(5))
                .WithConnectionModeDirect()
                .WithApplicationName("vera")
                .WithThrottlingRetryOptions(TimeSpan.FromSeconds(1), 5)
                .Build();

            if (!string.IsNullOrEmpty(cosmosContainerInvoices))
            {
                collection.AddSingleton<IInvoiceStore>(provider => new CosmosInvoiceStore(
                    cosmosClient.GetContainer(cosmosDatabase, cosmosContainerInvoices)
                ));
            }

            if (!string.IsNullOrEmpty(cosmosContainerCompanies))
            {
                collection.AddTransient<ICompanyStore>(provider => new CosmosCompanyStore(
                    cosmosClient.GetContainer(cosmosDatabase, cosmosContainerCompanies)
                ));

                collection.AddTransient<IAccountStore>(provider => new CosmosAccountStore(
                    cosmosClient.GetContainer(cosmosDatabase, cosmosContainerCompanies)
                ));

                collection.AddTransient<IUserStore>(provider => new CosmosUserStore(
                    cosmosClient.GetContainer(cosmosDatabase, cosmosContainerUsers)
                ));
            }
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