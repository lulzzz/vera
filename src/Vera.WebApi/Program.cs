using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Vera.Bootstrap;

namespace Vera.WebApi
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host..");

                var host = CreateHostBuilder(args).Build();

                await ConfigureCosmos(host);

                await host.RunAsync();

                return 0;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "failed to start");

                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(wb => wb.UseStartup<Startup>())
                .UseSerilog()
                .UseVera();
        }

        private static async Task ConfigureCosmos(IHost host)
        {
            using var scope = host.Services.CreateScope();

            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var cosmosOptions = config
                .GetSection(CosmosOptions.Section)
                .Get<CosmosOptions>();

            var cosmosContainerOptions = config
                .GetSection(CosmosContainerOptions.Section)
                .Get<CosmosContainerOptions>() ?? new CosmosContainerOptions();

            var client = scope.ServiceProvider.GetRequiredService<CosmosClient>();

            var response = await client.CreateDatabaseIfNotExistsAsync(cosmosOptions.Database);
            var db = response.Database;

            const string partitionKeyPath = "/PartitionKey";

            await db.CreateContainerIfNotExistsAsync(cosmosContainerOptions.Companies, partitionKeyPath);
            await db.CreateContainerIfNotExistsAsync(cosmosContainerOptions.Users, partitionKeyPath);
            await db.CreateContainerIfNotExistsAsync(cosmosContainerOptions.Invoices, partitionKeyPath);
            await db.CreateContainerIfNotExistsAsync(cosmosContainerOptions.Audits, partitionKeyPath);
        }
    }
}