using System.Collections.Generic;

namespace SkyApm.Tracing.Segments
{
    public class LoggerContext
    {
        public Dictionary<string, object> Logs { get; set; }

        public SegmentContext SegmentContext { get; set; }
    }
}
