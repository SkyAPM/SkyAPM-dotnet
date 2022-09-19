using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkyApm.Agent.GeneralHost;

namespace SkyApm.Sample.GenericHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .ConfigureServices(services => services.AddHostedService<Worker>().AddHostedService<LogWorker>()
                )
                .AddSkyAPM();
    }
}