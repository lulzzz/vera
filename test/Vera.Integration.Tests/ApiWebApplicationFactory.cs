using Bogus;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Vera.Azure;
using Vera.Concurrency;
using Vera.Host;

namespace Vera.Integration.Tests
{
    public class ApiWebApplicationFactory : WebApplicationFactory<Startup>
    {
        public Setup CreateSetup()
        {
            var client = CreateClient();

            var channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
            {
                HttpClient = client,
                ThrowOperationCanceledOnCancellation = true
            });

            return new Setup(channel, new Faker());
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace with static instance so locking works over tests running in parallel
                services.Replace(ServiceDescriptor.Singleton<ILocker>(new StaticLocker()));
            });
            
            base.ConfigureWebHost(builder);
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = builder
                .Build()
                .ConfigureCosmos();

            host.Start();

            return host;
        }
    }
}