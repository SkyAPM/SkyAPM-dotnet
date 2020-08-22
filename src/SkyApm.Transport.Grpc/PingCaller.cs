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

using System;
using System.Threading;
using System.Threading.Tasks;
using SkyApm.Config;
using SkyApm.Logging;
using SkyWalking.NetworkProtocol;
using SkyApm.Transport.Grpc.Common;

namespace SkyApm.Transport.Grpc
{
    public class PingCaller : IPingCaller
    {
        private readonly TransportConfig _transportConfig;
        private readonly IPingCaller _pingCallerV8;

        public PingCaller(ConnectionManager connectionManager, ILoggerFactory loggerFactory,
            IConfigAccessor configAccessor)
        {
            _transportConfig = configAccessor.Get<TransportConfig>();
            _pingCallerV8 = new V8.PingCaller(connectionManager, loggerFactory, configAccessor);
        }

        public async Task PingAsync(PingRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_transportConfig.ProtocolVersion == ProtocolVersions.V8)
                await _pingCallerV8.PingAsync(request, cancellationToken);
        }
    }
}