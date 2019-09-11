using Grpc.Core;
using GrpcGreeter;
using Microsoft.Extensions.Hosting;
using SkyApm.Agent.GeneralHost;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Sample.GrpcServer
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new HostBuilder()
                .UseEnvironment(environmentName)
                .AddSkyAPM()
                .ConfigureServices((hostContext, services) =>
                {
                    var startUp = new Startup(hostContext.Configuration);
                    var provider = startUp.ConfigureServices(services);
                    startUp.Use(provider);
                });
        }
    }
}
