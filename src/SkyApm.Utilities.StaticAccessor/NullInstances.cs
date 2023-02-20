using SkyApm.Tracing.Segments;

namespace SkyApm.Utilities.StaticAccessor
{
    public static class NullInstances
    {
        public static readonly SegmentSpan SegmentSpan = new SegmentSpan("NULL", SpanType.Local);

        public static readonly SegmentContext SegmentContext = new SegmentContext(string.Empty, string.Empty, false, string.Empty, string.Empty, "NULL", SpanType.Local);
    }
}
