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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SkyApm.Config;
using SkyApm.Logging;

namespace SkyApm.Transport.Kafka
{
    public class LogReporter : ILogReporter
    {
        private readonly InstrumentConfig _instrumentConfig;
        private readonly TransportConfig _transportConfig;
        private readonly ILogReporter _logReporter;

        public LogReporter(ILoggerFactory loggerFactory,
            IConfigAccessor configAccessor)
        {
            _instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _transportConfig = configAccessor.Get<TransportConfig>();
            _logReporter = new V8.LogReporter(loggerFactory, configAccessor);
        }

        public async Task ReportAsync(IReadOnlyCollection<LogRequest> logRequests,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_instrumentConfig != null && ! _instrumentConfig.LogActive)
            {
                return;
            }
            if (_transportConfig.ProtocolVersion == ProtocolVersions.V8)
                await _logReporter.ReportAsync(logRequests);
        }
    }
}
