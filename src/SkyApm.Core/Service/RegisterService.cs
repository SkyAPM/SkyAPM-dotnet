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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Transport;

namespace SkyApm.Service
{
    public class RegisterService : ExecutionService
    {
        private readonly InstrumentConfig _config;
        private readonly IServiceRegister _serviceRegister;
        private readonly TransportConfig _transportConfig;

        public RegisterService(IConfigAccessor configAccessor, IServiceRegister serviceRegister,
            IRuntimeEnvironment runtimeEnvironment, ILoggerFactory loggerFactory) : base(runtimeEnvironment,
            loggerFactory)
        {
            _serviceRegister = serviceRegister;
            _config = configAccessor.Get<InstrumentConfig>();
            _transportConfig = configAccessor.Get<TransportConfig>();
        }

        protected override TimeSpan DueTime { get; } = TimeSpan.Zero;

        protected override TimeSpan Period { get; } = TimeSpan.FromSeconds(30);

        protected override bool CanExecute() => !RuntimeEnvironment.Initialized;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await ReportServiceInstancePropertiesAsync(cancellationToken);
        }

        private async Task ReportServiceInstancePropertiesAsync(CancellationToken cancellationToken)
        {
            var properties = new AgentOsInfoRequest
            {
                HostName = DnsHelpers.GetHostName(),
                IpAddress = DnsHelpers.GetIpV4s(),
                OsName = PlatformInformation.GetOSName(),
                ProcessNo = Process.GetCurrentProcess().Id,
                Language = "dotnet"
            };
            var request = new ServiceInstancePropertiesRequest
            {
                ServiceId = _config.ServiceName ?? _config.ApplicationCode,
                ServiceInstanceId = _config.ServiceInstanceName,
                Properties = properties
            };
            var result = await Polling(3,
                    () => _serviceRegister.ReportInstancePropertiesAsync(request, cancellationToken),
                    cancellationToken);
            if (result && RuntimeEnvironment is RuntimeEnvironment environment)
            {
                environment.Initialized = true;
                Logger.Information($"Reported Service Instance Properties[Service={request.ServiceId},InstanceId={request.ServiceInstanceId}].");
            }
        }

        private static async Task<NullableValue> Polling(int retry, Func<Task<NullableValue>> execute, CancellationToken cancellationToken)
        {
            return await Polling(retry, execute, result => result.HasValue, NullableValue.Null, cancellationToken);
        }

        private static async Task<bool> Polling(int retry, Func<Task<bool>> execute, CancellationToken cancellationToken)
        {
            return await Polling(retry, execute, result => result, false, cancellationToken);
        }

        private static async Task<T> Polling<T>(int retry, Func<Task<T>> execute, Func<T,bool> successPredicate, T failureResult,
            CancellationToken cancellationToken)
        {
            var index = 0;
            while (index++ < retry)
            {
                var value = await execute();
                if (successPredicate(value))
                {
                    return value;
                }

                await Task.Delay(500, cancellationToken);
            }

            return failureResult;
        }
    }
}