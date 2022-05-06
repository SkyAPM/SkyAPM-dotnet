using SkyApm.Config;
using SkyApm.Diagnostics.Logging;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddPushSkyApmLogger(this IServiceCollection services, Action<LogPushSkywalkingConfig> action)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            LogPushSkywalkingConfig logPushSkywalking = new LogPushSkywalkingConfig();
            action.Invoke(logPushSkywalking);
            services.Configure(action);

            services.Add(new ServiceDescriptor(typeof(ISkyApmLogger<>), typeof(SkyApmLogger<>), ServiceLifetime.Singleton));
            return services;
        }
    }
}
