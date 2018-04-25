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
using System.Threading.Tasks;
using Grpc.Core;
using SkyWalking.Logging;

namespace SkyWalking.Remote
{
    public class GrpcConnection
    {
        private static readonly ILogger _Logger = LogManager.GetLogger<GrpcConnection>();

        private readonly Channel _internalChannel;
        private GrpcConnectionState _state = GrpcConnectionState.Idle;
        private readonly string _connectionId = Guid.NewGuid().ToString();

        public string ConnectionId => _connectionId;

        public Channel GrpcChannel => _internalChannel;

        public GrpcConnectionState State => _state;

        public GrpcConnection(string server)
        {
            _internalChannel = new Channel(server, ChannelCredentials.Insecure);
        }

        public async Task<bool> ConnectAsync()
        {
            CheckState();
            if (_state == GrpcConnectionState.Ready)
            {
                return true;
            }
            _state = GrpcConnectionState.Connecting;
            try
            {
                // default timeout = 5s
                var deadLine = DateTime.UtcNow.AddSeconds(5);
                await _internalChannel.ConnectAsync(deadLine);
                _state = GrpcConnectionState.Ready;
                _Logger.Info($"Grpc channel connect success, server:{_internalChannel.Target}");
            }
            catch (TaskCanceledException ex)
            {
                _state = GrpcConnectionState.Failure;
                _Logger.Warning($"Grpc channel connect timeout.{ex.Message}");
            }
            catch (Exception ex)
            {
                _state = GrpcConnectionState.Failure;
                _Logger.Warning($"Grpc channel connect fail.{ex.Message}");
            }

            return _state == GrpcConnectionState.Ready;
        }

        public async Task ShutdowmAsync()
        {
            try
            {
                await _internalChannel.ShutdownAsync();
            }
            catch (Exception e)
            {
                _Logger.Error("Grpc channel shutdown fail.", e);
            }
            finally
            {
                _state = GrpcConnectionState.Shutdown;
            }
        }

        public void CheckState()
        {
            var channelState = (int) _internalChannel.State;
            if ((int) _state == channelState)
            {
                return;
            }

            _Logger.Debug($"Grpc channel state changed. {_state} -> {_internalChannel.State}");

            _state = (GrpcConnectionState)channelState;
        }
    }
}