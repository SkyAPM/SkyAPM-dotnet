using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SkyApm.Sample.GenericHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .ConfigureServices(services => services.AddHostedService<Worker>())
                .AddSkyAPM();
    }
}