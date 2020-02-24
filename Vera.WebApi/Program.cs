using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

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