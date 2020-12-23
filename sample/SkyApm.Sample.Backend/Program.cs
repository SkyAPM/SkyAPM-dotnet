using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace SkyApm.Sample.Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildHost(args).Run();
        }

#if NETCOREAPP2_1

        public static IWebHost BuildHost(string[] args) =>
            WebHost.CreateDefaultBuilder<Startup>(args)
                .Build();

#else

        public static IHost BuildHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseStartup<Startup>();
                }).Build();

#endif
    }
}