namespace SkyApm.Tracing.Segments
{
    public static class SkySegmentExtensions
    {
        public static CrossThreadCarrier GetCrossThreadCarrier(this TraceSegment segment, int spanId)
        {
            return new CrossThreadCarrier
            {
                Reference = Reference.CrossThread,
                TraceId = segment.TraceId,
                ParentSegmentId = segment.SegmentId,
                ParentSpanId = spanId,
                ParentServiceId = segment.ServiceId,
                ParentServiceInstanceId = segment.ServiceInstanceId,
                ParentEndpoint = segment.FirstSpan.OperationName,
                Sampled = segment.Sampled
            };
        }
    }
}
