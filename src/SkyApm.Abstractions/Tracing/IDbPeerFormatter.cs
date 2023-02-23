using System.Data.Common;

namespace SkyApm.Tracing
{
    public interface IDbPeerFormatter
    {
        bool Match(DbConnection connection);

        string GetPeer(DbConnection connection);
    }
}
