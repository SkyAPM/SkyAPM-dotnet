using SkyApm.Tracing.Segments;

namespace SkyApm.Tracing
{
    public static class SkyTracingExtensions
    {
        public static SegmentReference ToReference(this ICarrier carrier, Reference reference = Reference.CrossProcess)
        {
            if (carrier == null || !carrier.HasValue) return null;

            return new SegmentReference
            {
                Reference = reference,
                EntryEndpoint = carrier.EntryEndpoint,
                NetworkAddress = carrier.NetworkAddress.HasValue ? carrier.NetworkAddress : "UNKNOW",
                ParentEndpoint = carrier.ParentEndpoint,
                ParentSpanId = carrier.ParentSpanId,
                ParentSegmentId = carrier.ParentSegmentId,
                EntryServiceInstanceId = carrier.EntryServiceInstanceId,
                ParentServiceInstanceId = carrier.ParentServiceInstanceId,
                TraceId = carrier.TraceId,
                ParentServiceId = carrier.ParentServiceId,
            };
        }
    }
}
