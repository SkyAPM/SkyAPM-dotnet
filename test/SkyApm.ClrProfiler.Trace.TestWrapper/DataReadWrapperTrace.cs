using System;
using System.Linq;
using SkyApm.ClrProfiler.Trace.Utils;
using SkyApm.Tracing;

namespace SkyApm.ClrProfiler.Trace.TestWrapper
{
    public class DataReadWrapperTrace : AbsMethodWrapper
    {
        private const string TypeName = "SkyApm.ClrProfiler.Trace.Test.TraceAgentTest";
        private static readonly string[] AssemblyNames = { "SkyApm.ClrProfiler.Trace.Test" };

        private readonly ITracingContext _tracingContext;

        public DataReadWrapperTrace(ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }

        public override AfterMethodDelegate BeginWrapMethod(TraceMethodInfo traceMethodInfo)
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
                if (traceMethodInfo.MethodBase.Name == "DataRead" ||
                    traceMethodInfo.MethodBase.Name == "Test1" ||
                    traceMethodInfo.MethodBase.Name == "Test2" ||
                    traceMethodInfo.MethodBase.Name == "StaticNoReturnTest")
                {
                    return true;
                }
            }
            return false;
        }
    }

}
