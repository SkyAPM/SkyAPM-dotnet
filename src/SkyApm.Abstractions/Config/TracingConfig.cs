using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Config
{
    [Config("SkyWalking", "Tracing")]
    public class TracingConfig
    {
        public int ExceptionMaxDepth { get; set; } = 3;
    }
}
