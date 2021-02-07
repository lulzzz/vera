using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Concurrency;
using Vera.Invoices;
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
                // Components
                collection.AddTransient<ITokenFactory, RandomTokenFactory>();
                collection.AddTransient<IPasswordStrategy, Pbkdf2PasswordStrategy>();
                collection.AddTransient<IAccountComponentFactoryCollection, AccountComponentFactoryCollection>();

                // Stores
                collection.AddSingleton<IBlobStore, TemporaryBlobStore>();
                collection.AddSingleton<ILocker, NullLocker>();

                // Services
                collection.AddTransient<IUserRegisterService, UserRegisterService>();
                collection.AddTransient<IInvoiceProcessor, InvoiceProcessor>();
            });

            // Registration of all the certification implementations
            builder.UseVeraPortugal();

            return builder;
        }
    }
}