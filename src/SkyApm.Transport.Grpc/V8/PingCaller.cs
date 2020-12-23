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
using SkyWalking.NetworkProtocol.V3;
using SkyApm.Transport.Grpc.Common;

namespace SkyApm.Transport.Grpc.V8
{
    internal class PingCaller : IPingCaller
    {
        private readonly ConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly GrpcConfig _config;

        public PingCaller(ConnectionManager connectionManager, ILoggerFactory loggerFactory,
            IConfigAccessor configAccessor)
        {
            _connectionManager = connectionManager;
            _config = configAccessor.Get<GrpcConfig>();
            _logger = loggerFactory.CreateLogger(typeof(PingCaller));
        }

        public Task PingAsync(PingRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_connectionManager.Ready)
            {
                return Task.CompletedTask;
            }

            var connection = _connectionManager.GetConnection();
            return new Call(_logger, _connectionManager).Execute(async () =>
                {
                    var client = new ManagementService.ManagementServiceClient(connection);
                    await client.keepAliveAsync(new InstancePingPkg
                    {
                        Service = request.ServiceName,
                        ServiceInstance = request.InstanceId,
                    }, _config.GetMeta(), _config.GetTimeout(), cancellationToken);
                },
                () => ExceptionHelpers.PingError);
        }
    }
}