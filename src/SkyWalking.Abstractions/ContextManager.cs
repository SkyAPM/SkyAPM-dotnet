using System;
using System.Threading;

namespace SkyWalking.Abstractions
{
    /// <summary>
    /// Context manager controls the whole context of tracing. Since .NET server application runs as same as Java,
    /// We also provide the CONTEXT propagation based on ThreadLocal mechanism.
    /// Meaning, each segment also related to singe thread.
    /// </summary>
    public static class ContextManager
    {
        private static ThreadLocal<AbstractTracerContext> CONTEXT = new ThreadLocal<AbstractTracerContext>();

        private static AbstractTracerContext GetOrCreate(String operationName, bool forceSampling)
        {
            if (!CONTEXT.IsValueCreated)
            {
                return null;
            }
            else
            {
                return null;
            }

        }
    }
}
