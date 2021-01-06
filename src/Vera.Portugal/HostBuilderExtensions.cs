using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Concurrency;
using Vera.Configuration;
using Vera.Dependencies;
using Vera.Stores;

namespace Vera.Portugal
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseVeraPortugal(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, collection) =>
                {
                    collection.AddSingleton<IAccountComponentFactory>(provider =>
                        new AccountComponentFactory(
                            provider.GetService<IInvoiceStore>(),
                            provider.GetService<ILocker>()
                        )
                    );
                });

            return builder;
        }
    }
}