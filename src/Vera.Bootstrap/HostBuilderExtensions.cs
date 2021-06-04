using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Concurrency;
using Vera.Dependencies;
using Vera.Models;
using Vera.Norway;
using Vera.Portugal;
using Vera.Reports;
using Vera.Security;
using Vera.Services;
using Vera.Stores;
using Vera.Invoices;
using Vera.Germany;
using Vera.Sweden;
using Vera.Sweden.InfrasecHttpClient;

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
                collection.AddTransient<IRegisterReportGenerator, RegisterReportGenerator>();
                collection.AddTransient<PeriodManager>();
                collection.AddTransient<IInvoiceHandlerFactory, InvoiceHandlerFactory>();

                // Stores
                collection.AddSingleton<IBlobStore, TemporaryBlobStore>();
                collection.AddSingleton<ILocker, InMemoryLocker>();

                // Services
                collection.AddTransient<IUserRegisterService, UserRegisterService>();
                collection.AddSingleton<IBucketGenerator<RegisterReport>, ReportBucketGenerator>();
                collection.AddTransient<IReportHandlerFactory, ReportHandlerFactory>();
                collection.AddTransient<IDateProvider, RealLifeDateProvider>();

                // Adds support for injecting the IHttpClientFactory etc.
                collection.AddHttpClient();
            });

            // Registration of all the certification implementations
            builder
                .UseVeraPortugal()
                .UseVeraNorway()
                .UseVeraSweden()
                .UseVeraGermany();

            return builder;
        }
    }
}
