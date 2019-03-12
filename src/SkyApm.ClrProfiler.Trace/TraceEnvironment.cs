using SkyApm.ClrProfiler.Trace.Constants;
using System;

namespace SkyApm.ClrProfiler.Trace
{
    public class TraceEnvironment
    {
        private readonly Lazy<string> LazyProfilerHome = new Lazy<string>(() => Environment.GetEnvironmentVariable(TraceConstant.PROFILER_HOME));
        public static readonly TraceEnvironment Instance = new TraceEnvironment();
        private TraceEnvironment()
        {

        }

        public string GetProfilerHome()
        {
            return LazyProfilerHome.Value;
        }
    }
}
