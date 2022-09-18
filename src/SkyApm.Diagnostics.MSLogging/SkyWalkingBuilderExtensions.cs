using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SkyApm.Transport;
using SkyApm.Utilities.DependencyInjection;

namespace SkyApm.Diagnostics.MSLogging
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddMSLogging(this SkyApmExtensions extensions)
        {
            extensions.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SkyApmLoggerProvider>());
            return extensions;
        }
    }
}