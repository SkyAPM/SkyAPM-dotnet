using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SkyApm.AspNetCore.Diagnostics;
using SkyApm.Diagnostics.MassTransit;
using System.Diagnostics;

[assembly: HostingStartup(typeof(MasstransitSkyApmHostingStartup))]
namespace SkyApm.Diagnostics.MassTransit
{
    internal class MasstransitSkyApmHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            //config the ActivityListener 
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;
            ActivitySource.AddActivityListener(new ActivityListener()
            {
                ShouldListenTo = (source) => source.Name == "MassTransit",
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = activity => { },
                ActivityStopped = activity => { }
            });
            builder.ConfigureServices(
                services => services.AddSkyAPM
                (
                    ext =>
                    {
                        ext.AddAspNetCoreHosting();
                        ext.AddMasstransit();
                    }
                ));
        }
    }
}
