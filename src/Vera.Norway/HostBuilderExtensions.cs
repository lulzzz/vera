using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Dependencies;

namespace Vera.Norway
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseVeraNorway(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, collection) =>
            {
                collection.AddSingleton<IAccountComponentFactory, AccountComponentFactory>();
            });

            return builder;
        }
    }
}
