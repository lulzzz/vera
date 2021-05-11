using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Dependencies;
using Vera.Invoices;
using Vera.Portugal.Invoices;

namespace Vera.Portugal
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseVeraPortugal(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, collection) =>
            {
                collection.AddSingleton<IAccountComponentFactory, AccountComponentFactory>();
                collection.AddTransient<IInvoiceHandlerFactory, PortugalInvoiceHandlerFactory>();
            });

            return builder;
        }
    }
}