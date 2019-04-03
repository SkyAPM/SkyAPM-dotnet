using System;
using System.Linq;
using SkyApm.ClrProfiler.Trace.Utils;
using SkyApm.Tracing;

namespace SkyApm.ClrProfiler.Trace.TestWrapper
{
    // ReSharper disable once InconsistentNaming
    public class ILReWriteWrapperTrace : AbsMethodWrapper
    {
        private static readonly string[] TypeNames =
        {
            "SkyApm.ClrProfiler.Trace.Test.ILReWriteTest",
            "SkyApm.ClrProfiler.Trace.Test.ILReWriteTest2"
        };

        private static readonly string[] AssemblyNames = { "SkyApm.ClrProfiler.Trace.Test" };

        private readonly ITracingContext _tracingContext;

        public ILReWriteWrapperTrace(ITracingContext tracingContext)
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
            if (AssemblyNames.Contains(assemblyName) && TypeNames.Contains(invocationTargetType.FullName))
            {
                if (traceMethodInfo.MethodBase.Name == "Test1" ||
                    traceMethodInfo.MethodBase.Name == "Test2" ||
                    traceMethodInfo.MethodBase.Name == "StaticNoReturn")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
