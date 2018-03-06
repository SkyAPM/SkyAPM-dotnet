using System;
using System.Collections.Generic;
using System.Text;

namespace SkyWalking.Context.Trace
{
    public enum SpanLayer
    {
        DB = 1,
        RPC_FRAMEWORK = 2,
        HTTP = 3,
        MQ,
        CACHE = 5
    }
}
