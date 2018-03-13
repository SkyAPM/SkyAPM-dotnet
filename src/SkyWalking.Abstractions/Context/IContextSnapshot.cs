using SkyWalking.Context.Ids;

namespace SkyWalking.Context
{
    public interface IContextSnapshot
    {
        string EntryOperationName { get; set; }

        string ParentOperationName { get; set; }

        DistributedTraceId DistributedTraceId { get; }

        int EntryApplicationInstanceId { get; set; }

        int SpanId { get; }

        bool IsFromCurrent { get; }

        bool IsValid { get; }

        ID TraceSegmentId { get; }

        int EntryOperationId { set; }
        
        int ParentOperationId { set; }
    }
}