using SkyApm.Config;
using SkyApm.Tracing;

namespace SkyApm.Utilities.StaticAccessor
{
    public static class SkyApmInstances
    {
        public static ITracingContext TracingContext { get; internal set; } = new NullTracingContext();

        public static IConfigAccessor ConfigAccessor { get; internal set; } = new NullConfigAccessor();
    }
}