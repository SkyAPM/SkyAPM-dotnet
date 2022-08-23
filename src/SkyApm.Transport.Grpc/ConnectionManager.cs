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

using Grpc.Net.Client;
using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Transport.Grpc.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SkyApm.Transport.Grpc
{
    public class ConnectionManager
    {
        private readonly Random _random = new Random();
        private readonly AsyncLock _lock = new AsyncLock();

        private readonly ILogger _logger;
        private readonly GrpcConfig _config;

        private volatile GrpcChannel _channel;
        private volatile ConnectionState _state;
        private volatile string _server;

        public bool Ready => _channel != null && _state == ConnectionState.Connected;

        public ConnectionManager(ILoggerFactory loggerFactory, IConfigAccessor configAccessor)
        {
            _logger = loggerFactory.CreateLogger(typeof(ConnectionManager));
            _config = configAccessor.Get<GrpcConfig>();
        }

        public async Task ConnectAsync()
        {
            using (await _lock.LockAsync())
            {
                if (Ready)
                {
                    return;
                }

                if (_channel != null)
                {
                    await ShutdownAsync();
                }

                EnsureServerAddress();
                
                try
                {
                    //https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.0#call-insecure-grpc-services-with-net-core-client-2
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                    
                    _channel = GrpcChannel.ForAddress(_server);
                    _state = ConnectionState.Connected;
                    _logger.Information($"Connected server[{_channel.Target}].");
                }
                catch (TaskCanceledException ex)
                {
                    _state = ConnectionState.Failure;
                    _logger.Error($"Connect server timeout.", ex);
                }
                catch (Exception ex)
                {
                    _state = ConnectionState.Failure;
                    _logger.Error($"Connect server fail.", ex);
                }
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                await _channel?.ShutdownAsync();
                _logger.Information($"Shutdown connection[{_channel.Target}].");
            }
            catch (Exception e)
            {
                _logger.Error($"Shutdown connection fail.", e);
            }
            finally
            {
                _state = ConnectionState.Failure;
            }
        }

        public void Failure(Exception exception)
        {
            var currentState = _state;

            if (ConnectionState.Connected == currentState)
            {
                _logger.Warning($"Connection state changed. {exception.Message}");
            }

            _state = ConnectionState.Failure;
        }

        public GrpcChannel GetConnection()
        {
            if (Ready) return _channel;
            _logger.Debug("Not found available gRPC connection.");
            return null;
        }

        private void EnsureServerAddress()
        {
            var servers = _config.GetServers();
            if (servers.Length == 1)
            {
                _server = servers[0];
                return;
            }

            if (_server != null)
            {
                servers = servers.Where(x => x != _server).ToArray();
            }

            var index = _random.Next() % servers.Length;
            _server = servers[index];
        }
    }

    public enum ConnectionState
    {
        Idle,
        Connecting,
        Connected,
        Failure
    }
}