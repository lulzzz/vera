using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Vera.Bootstrap;
using Vera.Host;

namespace Vera.Integration.Tests
{
    public class ApiWebApplicationFactory : WebApplicationFactory<Startup>
    {
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