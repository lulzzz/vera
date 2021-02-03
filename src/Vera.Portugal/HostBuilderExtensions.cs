using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Dependencies;

namespace Vera.Portugal
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseVeraPortugal(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, collection) =>
            {
                collection.AddSingleton<IAccountComponentFactory>(_ => new AccountComponentFactory());
            });

            return builder;
        }
    }
}