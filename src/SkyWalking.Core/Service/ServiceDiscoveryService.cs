/*
 * Licensed to the OpenSkywalking under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkyWalking.Config;
using SkyWalking.Logging;
using SkyWalking.Transport;
using SkyWalking.Utils;

namespace SkyWalking.Service
{
    public class ServiceDiscoveryService : InstrumentationService
    {
        private readonly InstrumentationConfig _config;

        protected override TimeSpan DueTime { get; } = TimeSpan.Zero;

        protected override TimeSpan Period { get; } = TimeSpan.FromSeconds(30);

        public ServiceDiscoveryService(IConfigAccessor configAccessor, IInstrumentationClient client, IRuntimeEnvironment runtimeEnvironment,
            IInstrumentationLoggerFactory loggerFactory)
            : base(client, runtimeEnvironment, loggerFactory)
        {
            _config = configAccessor.Get<InstrumentationConfig>();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await RegisterApplication(cancellationToken);
            await RegisterApplicationInstance(cancellationToken);
            await Heartbeat(cancellationToken);
        }

        protected override bool CanExecute() => true;

        private async Task RegisterApplication(CancellationToken cancellationToken)
        {
            if (!_runtimeEnvironment.ApplicationId.HasValue)
            {
                var value = await Polling(3, () => _instrumentation.RegisterApplicationAsync(_config.ApplicationCode, cancellationToken), cancellationToken);
                if (value.HasValue && _runtimeEnvironment is RuntimeEnvironment environment)
                {
                    environment.ApplicationId = value;
                }
            }
        }

        private async Task RegisterApplicationInstance(CancellationToken cancellationToken)
        {
            if (_runtimeEnvironment.ApplicationId.HasValue && !_runtimeEnvironment.ApplicationInstanceId.HasValue)
            {
                var osInfoRequest = new AgentOsInfoRequest
                {
                    HostName = DnsHelpers.GetHostName(),
                    IpAddress = DnsHelpers.GetIpV4s(),
                    OsName = PlatformInformation.GetOSName(),
                    ProcessNo = Process.GetCurrentProcess().Id
                };
                var value = await Polling(3,
                    () => _instrumentation.RegisterApplicationInstanceAsync(_runtimeEnvironment.ApplicationId.Value, _runtimeEnvironment.AgentUUID,
                        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), osInfoRequest, cancellationToken), cancellationToken);
                if (value.HasValue && _runtimeEnvironment is RuntimeEnvironment environment)
                {
                    environment.ApplicationInstanceId = value;
                }
            }
        }

        private static async Task<NullableValue> Polling(int retry, Func<Task<NullableValue>> execute, CancellationToken cancellationToken)
        {
            var index = 0;
            while (index++ < retry)
            {
                var value = await execute();
                if (value.HasValue)
                {
                    return value;
                }

                await Task.Delay(500, cancellationToken);
            }

            return NullableValue.Null;
        }

        private async Task Heartbeat(CancellationToken cancellationToken)
        {
            if (_runtimeEnvironment.Initialized)
            {
                try
                {
                    await _instrumentation.HeartbeatAsync(_runtimeEnvironment.ApplicationInstanceId.Value, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.Error("Heartbeat error.", e);
                }
            }
        }
    }
}