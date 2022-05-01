using SkyApm.Config;
using SkyApm.Tracing;

namespace SkyApm.Utilities.StaticAccessor
{
    public static class SkyApmInstances
    {
        public static ITracingContext TracingContext { get; internal set; } = NullInstances.TracingContext;

        public static TracingConfig TracingConfig { get; internal set; } = new TracingConfig();
    }
}