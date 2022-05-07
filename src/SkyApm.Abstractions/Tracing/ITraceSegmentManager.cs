using SkyApm.Common;
using SkyApm.Tracing.Segments;

namespace SkyApm.Tracing
{
    public interface ITraceSegmentManager
    {
        SegmentSpan ActiveSpan { get; }

        SegmentSpan CreateEntrySpan(string operationName, ICarrier carrier, long startTimeMilliseconds = default);

        SegmentSpan CreateLocalSpan(string operationName, long startTimeMilliseconds = default);

        SegmentSpan CreateLocalSpan(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = default);

        SegmentSpan CreateExitSpan(string operationName, StringOrIntValue networkAddress, long startTimeMilliseconds = default);

        SegmentSpan CreateExitSpan(string operationName, StringOrIntValue networkAddress, CrossThreadCarrier carrier, long startTimeMilliseconds = default);

        TraceSegment StopSpan(SegmentSpan span, long endTimeMilliseconds = default);

        (TraceSegment, SegmentSpan) StopSpan(long endTimeMilliseconds = default);
    }
}
