using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Dependencies;
using Vera.Sweden.InfrasecHttpClient;
using Vera.Sweden.InfrasecHttpClient.Contracts;
using Vera.Sweden.RequestBuilders;
using Vera.Sweden.RequestBuilders.Contracts;
using Vera.Sweden.Validators;
using Vera.Sweden.Validators.Contracts;

namespace Vera.Sweden
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseVeraSweden(this IHostBuilder builder)
        {
            return builder.ConfigureServices((context, collection) =>
            {
                collection.AddSingleton<IAccountComponentFactory, AccountComponentFactory>();
                collection.AddSingleton<IInfrasecEnrollmentResponseValidator, InfrasecEnrollmentResponseValidator>();
                collection.AddSingleton<IInfrasecNewStationEnrollmentRequestBuilder, InfrasecNewStationEnrollmentRequestBuilder>();
                collection.AddSingleton<IInfrasecEnrollmentRequestValidator, InfrasecEnrollmentRequestValidator>();
                collection.AddSingleton<IInfrasecEnrollmentApiClientFactory, InfrasecEnrollmentApiClientFactory>();
            });
        }
    }
}
