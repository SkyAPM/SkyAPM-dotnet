using Microsoft.Extensions.DependencyInjection;
using SkyApm.Utilities.DependencyInjection;

namespace SkyApm.Diagnostics.MongoDB
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddMongoDB(this SkyApmExtensions extensions)
        {              
            extensions.Services.AddSingleton<ITracingDiagnosticProcessor, MongoDiagnosticsProcessor>();
            return extensions;
        }
    }
}
