using Grpc.Core;
using GrpcGreeter;
using Microsoft.Extensions.DependencyInjection;
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
            var host = CreateHostBuilder(args).Build();
            host.Services.StartGrpcServer();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new HostBuilder()
                .UseEnvironment(environmentName)
                .AddSkyAPM()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                });
        }
    }
}
