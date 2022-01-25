using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace SkyApm.Sample.FreeSqlSqlite
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
                    builder.UseStartup<Startup>();
                }).Build();

    }
}