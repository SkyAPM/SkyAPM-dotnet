using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Utilities.StaticAccessor
{
    internal class NullTracingContext : ITracingContext
    {
        public SegmentContext CreateEntrySegmentContext(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = 0) => NullInstances.SegmentContext;

        public SegmentContext CreateExitSegmentContext(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0) => NullInstances.SegmentContext;

        public SegmentContext CreateLocalSegmentContext(string operationName, long startTimeMilliseconds = 0) => NullInstances.SegmentContext;

        public void Release(SegmentContext segmentContext, long endTimeMilliseconds = 0)
        {

        }
    }
}
