namespace SkyApm.Tracing.Segments
{
    public static class SkySegmentsExtensions
    {
        public static CrossThreadCarrier GetCrossThreadCarrier(this SegmentSpan span)
        {
            if (span == null) return null;

            return new CrossThreadCarrier
            {
                Reference = Reference.CrossThread,
                TraceId = span.Segment.TraceId,
                ParentSegmentId = span.Segment.SegmentId,
                ParentSpanId = span.SpanId,
                ParentServiceId = span.Segment.ServiceId,
                ParentServiceInstanceId = span.Segment.ServiceInstanceId,
                ParentEndpoint = span.Segment.FirstSpan.OperationName,
                Sampled = span.Segment.Sampled
            };
        }

        public static CrossThreadCarrier GetCrossThreadCarrier(this SegmentContext segmentContext)
        {
            if (segmentContext == null) return null;

            return new CrossThreadCarrier
            {
                Reference = Reference.CrossThread,
                TraceId = segmentContext.TraceId,
                ParentSegmentId = segmentContext.SegmentId,
                ParentSpanId = segmentContext.Span.SpanId,
                ParentServiceId = segmentContext.ServiceId,
                ParentServiceInstanceId = segmentContext.ServiceInstanceId,
                ParentEndpoint = segmentContext.Span.OperationName,
                Sampled = segmentContext.Sampled
            };
        }

        public static CrossThreadCarrier GetCrossThreadCarrier(this SpanOrSegmentContext spanOrSegmentContext)
        {
            if (spanOrSegmentContext == null) return null;

            return spanOrSegmentContext.SegmentContext == null ?
                GetCrossThreadCarrier(spanOrSegmentContext.SegmentSpan) :
                GetCrossThreadCarrier(spanOrSegmentContext.SegmentContext);
        }
    }
}
