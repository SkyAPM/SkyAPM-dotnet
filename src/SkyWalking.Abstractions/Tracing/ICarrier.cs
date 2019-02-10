namespace SkyWalking.Tracing
{
    public interface ICarrier
    {
        bool? Sampled { get; }

        UniqueId TraceId { get; }

        UniqueId ParentSegmentId { get; }

        int ParentSpanId { get; }

        int ParentServiceInstanceId { get; }
        
        int EntryServiceInstanceId { get; }
        
        StringOrIntValue NetworkAddress { get; }
        
        StringOrIntValue EntryEndpoint { get; }
        
        StringOrIntValue ParentEndpoint { get; }
    }
}