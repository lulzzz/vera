using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Vera.Bootstrap;

namespace Vera.WebApi
{
    public class Program
    {
        public static int Main(string[] args)
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

                Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(wb => wb.UseStartup<Startup>())
                    .UseSerilog()
                    .UseVera()
                    .Build()
                    .Run();

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
    }
}