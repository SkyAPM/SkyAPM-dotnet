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
using SkyApm.Transport;

namespace SkyApm.Service
{
    public class PingService : ExecutionService
    {
        private readonly IPingCaller _pingCaller;
        private readonly TransportConfig _transportConfig;
        private readonly InstrumentConfig _instrumentConfig;

        public PingService(IConfigAccessor configAccessor, IPingCaller pingCaller,
            IRuntimeEnvironment runtimeEnvironment,
            ILoggerFactory loggerFactory) : base(
            runtimeEnvironment, loggerFactory)
        {
            _pingCaller = pingCaller;
            _transportConfig = configAccessor.Get<TransportConfig>();
            _instrumentConfig = configAccessor.Get<InstrumentConfig>();
        }

        protected override TimeSpan DueTime { get; } = TimeSpan.FromSeconds(30);
        protected override TimeSpan Period { get; } = TimeSpan.FromSeconds(60);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _pingCaller.PingAsync(
                    new PingRequest
                    {
                        ServiceName = _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                        InstanceId = _instrumentConfig.ServiceInstanceName
                    }, cancellationToken);
                Logger.Information($"Ping server @{DateTimeOffset.UtcNow}");
            }
            catch (Exception exception)
            {
                Logger.Error($"Ping server fail @{DateTimeOffset.UtcNow}", exception);
            }
        }
    }
}