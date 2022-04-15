using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Transport
{
    public class LoggerRequest
    {
        public Dictionary<string, object> Logs { get; set; }

        public SegmentRequest SegmentRequest { get; set; }
    }
}
