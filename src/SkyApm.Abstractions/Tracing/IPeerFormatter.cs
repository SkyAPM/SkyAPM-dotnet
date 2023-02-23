using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SkyApm.Tracing
{
    public interface IPeerFormatter
    {
        string GetDbPeer(DbConnection connection);
    }
}
