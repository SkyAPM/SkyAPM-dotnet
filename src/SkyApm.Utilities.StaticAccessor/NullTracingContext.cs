using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Utilities.StaticAccessor
{
    internal class NullTracingContext : ITracingContext
    {
        public SegmentSpan ActiveSpan => NullInstances.SegmentSpan;

        public TraceSegment ActiveSegment => NullInstances.TraceSegment;

        public SpanOrSegmentContext CurrentEntry => NullInstances.SpanOrSegmentContext;

        public SpanOrSegmentContext CurrentLocal => NullInstances.SpanOrSegmentContext;

        public SpanOrSegmentContext CurrentExit => NullInstances.SpanOrSegmentContext;

        public SpanOrSegmentContext CreateEntry(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SegmentContext CreateEntrySegmentContext(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentContext;
        }

        public SegmentSpan CreateEntrySpan(string operationName, ICarrierHeaderCollection carrierHeader, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentSpan;
        }

        public SpanOrSegmentContext CreateExit(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SpanOrSegmentContext CreateExit(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SegmentContext CreateExitSegmentContext(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentContext;
        }

        public SegmentContext CreateExitSegmentContext(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentContext;
        }

        public SegmentSpan CreateExitSpan(string operationName, string networkAddress, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentSpan;
        }

        public SegmentSpan CreateExitSpan(string operationName, string networkAddress, CrossThreadCarrier carrier, ICarrierHeaderCollection carrierHeader = null, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentSpan;
        }

        public SpanOrSegmentContext CreateLocal(string operationName, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SpanOrSegmentContext CreateLocal(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            return NullInstances.SpanOrSegmentContext;
        }

        public SegmentContext CreateLocalSegmentContext(string operationName, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentContext;
        }

        public SegmentContext CreateLocalSegmentContext(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentContext;
        }

        public SegmentSpan CreateLocalSpan(string operationName, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentSpan;
        }

        public SegmentSpan CreateLocalSpan(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            return NullInstances.SegmentSpan;
        }

        public void Finish(SpanOrSegmentContext spanOrSegmentContext)
        {
            
        }

        public void Release(SegmentContext segmentContext, long endTimeMilliseconds = 0)
        {
            
        }

        public void StopSpan(SegmentSpan span)
        {
            
        }

        public void StopSpan()
        {
            
        }
    }
}
