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
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Transport.Grpc.Common;

namespace SkyApm.Transport.Grpc
{
    public class ServiceRegister : IServiceRegister
    {
        private readonly TransportConfig _transportConfig;
        private readonly IServiceRegister _serviceRegisterV8;

        public ServiceRegister(ConnectionManager connectionManager, IConfigAccessor configAccessor,
            ILoggerFactory loggerFactory)
        {
            _transportConfig = configAccessor.Get<TransportConfig>();
            _serviceRegisterV8 = new V8.ServiceRegister(connectionManager, configAccessor, loggerFactory);
        }

        public async Task<bool> ReportInstancePropertiesAsync(ServiceInstancePropertiesRequest serviceInstancePropertiesRequest,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_transportConfig.ProtocolVersion == ProtocolVersions.V8)
                return await _serviceRegisterV8.ReportInstancePropertiesAsync(serviceInstancePropertiesRequest, cancellationToken);
            return true;
        }
    }
}