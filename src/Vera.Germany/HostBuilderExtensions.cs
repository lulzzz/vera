using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Dependencies;

namespace Vera.Germany
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseVeraGermany(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, collection) =>
            {
                collection.AddSingleton<IAccountComponentFactory, AccountComponentFactory>();
            });

            return builder;
        }
    }
}
