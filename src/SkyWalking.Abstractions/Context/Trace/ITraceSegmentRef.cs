using System;
using SkyWalking.NetworkProtocol;

namespace SkyWalking.Context.Trace
{
    public interface ITraceSegmentRef : IEquatable<ITraceSegmentRef>
    {
        string EntryOperationName { get; }
        
        int EntryOperationId { get; }
        
        int EntryApplicationInstance { get; }

        TraceSegmentReference Transform();
    }
}