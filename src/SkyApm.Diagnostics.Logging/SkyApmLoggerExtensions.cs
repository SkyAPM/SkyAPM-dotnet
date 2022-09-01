using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SkyApm.Diagnostics.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SkyApmLoggerExtensions
    {
        public static IServiceCollection AddSkyApmLogger(this IServiceCollection service)
        {
            service.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SkyApmLoggerProvider>());
            return service;
        }
    }
}

