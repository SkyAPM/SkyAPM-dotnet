using System;
using System.Linq;
using SkyApm.ClrProfiler.Trace.Utils;
using SkyApm.Tracing;

namespace SkyApm.ClrProfiler.Trace.Test
{
    public class DataReadWrapperTrace : AbsMethodWrapper
    {
        private const string TypeName = "SkyApm.ClrProfiler.Trace.Test.TraceAgentTest";
        private static readonly string[] AssemblyNames = new[] { "SkyApm.ClrProfiler.Trace.Test" };

        private readonly ITracingContext _tracingContext;

        public DataReadWrapperTrace(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _tracingContext = (ITracingContext)serviceProvider.GetService(typeof(ITracingContext));
        }

        public override EndMethodDelegate BeforeWrappedMethod(TraceMethodInfo traceMethodInfo)
        {
            return delegate (object returnValue, Exception ex)
            {
                DelegateHelper.AsyncMethodEnd(Leave, traceMethodInfo, ex, returnValue);
            };
        }

        private void Leave(TraceMethodInfo traceMethodInfo, object ret, Exception ex)
        {

        }

        public override bool CanWrap(TraceMethodInfo traceMethodInfo)
        {
            var invocationTargetType = traceMethodInfo.Type;
            var assemblyName = invocationTargetType.Assembly.GetName().Name;
            if (AssemblyNames.Contains(assemblyName) && TypeName == invocationTargetType.FullName)
            {
                if (traceMethodInfo.MethodBase.Name == "DataRead")
                {
                    return true;
                }
            }
            return false;
        }
    }

}
