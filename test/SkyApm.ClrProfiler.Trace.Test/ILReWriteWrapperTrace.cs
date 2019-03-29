using System;
using System.Linq;
using SkyApm.ClrProfiler.Trace.Utils;
using SkyApm.Tracing;

namespace SkyApm.ClrProfiler.Trace.Test
{
    // ReSharper disable once InconsistentNaming
    public class ILReWriteWrapperTrace : AbsMethodWrapper
    {
        private const string TypeName = "SkyApm.ClrProfiler.Trace.Test.ILReWriteTest";
        private static readonly string[] AssemblyNames = { "SkyApm.ClrProfiler.Trace.Test" };

        private readonly ITracingContext _tracingContext;

        public ILReWriteWrapperTrace(IServiceProvider serviceProvider) : base(serviceProvider)
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
            if (AssemblyNames.Contains(assemblyName) &&  invocationTargetType.FullName.StartsWith(TypeName))
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
