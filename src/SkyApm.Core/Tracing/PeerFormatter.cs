using SkyApm.Config;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;

namespace SkyApm.Tracing
{
    public class PeerFormatter : IPeerFormatter
    {
        private readonly ConcurrentDictionary<string, string> _peerMap = new ConcurrentDictionary<string, string>();

        private readonly IEnumerable<IDbPeerFormatter> _dbPeerFormatters;
        private readonly TracingConfig _tracingConfig;

        public PeerFormatter(IEnumerable<IDbPeerFormatter> dbPeerFormatters, IConfigAccessor configAccessor)
        {
            _dbPeerFormatters= dbPeerFormatters;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        public string GetDbPeer(DbConnection connection)
        {
            if (!_tracingConfig.JavaDbPeerFormat) return connection.DataSource;

            return _peerMap.GetOrAdd($"{connection.GetType()}_{connection.DataSource}", k =>
            {
                foreach (var formatter in _dbPeerFormatters)
                {
                    if (formatter.Match(connection))
                    {
                        return formatter.GetPeer(connection);
                    }
                }

                return connection.DataSource;
            });
        }
    }
}
