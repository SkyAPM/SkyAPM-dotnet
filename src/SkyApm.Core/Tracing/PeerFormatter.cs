using SkyApm.Config;
using System.Collections.Concurrent;
using System.Collections.Generic;
/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

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
            if (!_tracingConfig.DbPeerSimpleFormat) return connection.DataSource;

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
