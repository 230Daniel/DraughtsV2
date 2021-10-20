using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Draughts.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();
            
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(host.Services.GetRequiredService<IConfiguration>())
                .CreateLogger();

            try
            {
                Log.Information("Running host");
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Crashed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
