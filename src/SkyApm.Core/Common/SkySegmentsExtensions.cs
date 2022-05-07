using SkyApm.Common;

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
                Sampled = span.Segment.Sampled,
                NetworkAddress = DnsHelpers.GetIpV4OrHostName()
            };
        }
    }
}
