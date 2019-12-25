using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SkyApm.Sample.Frontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildHost(args).Run();
        }

        public static IHost BuildHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder =>
                {
                    builder
                        .UseStartup<Startup>()
                        .UseUrls("http://*:5001");
                })
                .Build();
    }
}