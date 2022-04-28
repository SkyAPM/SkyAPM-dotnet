using SkyApm.Tracing.Segments;
using System.Collections.Generic;

namespace SkyApm.Tracing
{
    public interface IAsyncSpanCombiner
    {
        TraceSegment[] Merge(IEnumerable<TraceSegment> segments);
    }
}
