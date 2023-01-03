using SkyApm.Tracing.Segments;
using System;
using System.Collections.Concurrent;

namespace SkyApm.Diagnostics.MassTransit.Observers
{
    public class ObserverSegmentContextDictionary
    {
        public static readonly ConcurrentDictionary<Guid, SegmentContext> Contexts = new ConcurrentDictionary<Guid, SegmentContext>();
    }
}