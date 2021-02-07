using System;
using System.Threading;
using Bogus;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Vera.Azure;
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