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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using SkyWalking.Config;
using SkyWalking.Logging;
using SkyWalking.NetworkProtocol.Trace;

namespace SkyWalking.Remote
{
    public class GrpcConnectionManager
    {
        private static readonly ILogger _logger = LogManager.GetLogger<GrpcConnectionManager>();
        private static readonly GrpcConnectionManager _client = new GrpcConnectionManager();

        public static GrpcConnectionManager Instance => _client;

        private ICollection<GrpcConnection> _connections = new List<GrpcConnection>();

        private GrpcConnectionManager()
        {
            foreach (var server in RemoteDownstreamConfig.Collector.gRPCServers)
            {
                _connections.Add(new GrpcConnection(server));
            }
        }

        public Task ConnectAsync()
        {
            foreach (var connection in _connections)
            {
                connection.ConnectAsync();
            }

            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            foreach (var connection in _connections)
            {
                connection.ShutdowmAsync();
            }

            return Task.CompletedTask;
        }

        public GrpcConnection GetAvailableConnection(object key)
        {
            var availableConnections = _connections.Where(x => x.State == GrpcConnectionState.Ready).ToArray();
            if (availableConnections.Length == 0)
            {
                _logger.Debug("Not found available connection.");
                throw new InvalidOperationException("Not found available connection.");
            }

            if (availableConnections.Length == 1)
            {
                return availableConnections[0];
            }

            var index = key.GetHashCode() % availableConnections.Length;
            return availableConnections[index];
        }
    }
}