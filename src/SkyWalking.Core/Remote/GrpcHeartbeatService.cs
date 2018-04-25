﻿/*
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
using System.Threading;
using System.Threading.Tasks;
using SkyWalking.Boot;
using SkyWalking.Config;
using SkyWalking.Context;
using SkyWalking.Dictionarys;
using SkyWalking.Logging;
using SkyWalking.NetworkProtocol;

namespace SkyWalking.Remote
{
    public class GrpcHeartbeatService : TimerService
    {
        private static readonly ILogger _logger = LogManager.GetLogger<GrpcHeartbeatService>();
        private readonly Random _random = new Random();
        protected override TimeSpan Interval { get; } = TimeSpan.FromMinutes(1);

        protected override async Task Execute(CancellationToken token)
        {
            if (DictionaryUtil.IsNull(RemoteDownstreamConfig.Agent.ApplicationInstanceId))
            {
                return;
            }

            GrpcConnection availableConnection = null;
            try
            {
                availableConnection =
                    GrpcConnectionManager.Instance.GetAvailableConnection(_random.Next());

                var instanceDiscoveryService =
                    new InstanceDiscoveryService.InstanceDiscoveryServiceClient(availableConnection.GrpcChannel);

                var heartbeat = new ApplicationInstanceHeartbeat
                {
                    ApplicationInstanceId = RemoteDownstreamConfig.Agent.ApplicationInstanceId,
                    HeartbeatTime = DateTime.UtcNow.GetTimeMillis()
                };

                await instanceDiscoveryService.heartbeatAsync(heartbeat);

                _logger.Debug($"{DateTime.Now} Heartbeat.");
            }
            catch (Exception e)
            {
                _logger.Warning($"{DateTime.Now} Heartbeat fail. {e.Message}");
                availableConnection?.CheckState();
            }
        }
    }
}