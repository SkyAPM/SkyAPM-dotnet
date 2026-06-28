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
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Transport.Grpc
{
    public class ConnectionManager
    {
        private const int DefaultConnectTimeoutMs = 10000;

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

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            using (await _lock.LockAsync())
            {
                // Bail out once the agent is shutting down so a late tick can't rebuild a channel after
                // ShutdownAsync has already disposed it (which nothing would dispose again).
                if (Ready || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    var servers = _config.GetServers();

                    // Reuse the channel across transient failures: a GrpcChannel is long-lived and
                    // re-establishes its underlying HTTP/2 connection on its own. Tearing it down and
                    // rebuilding on every failure was the root cause of the socket churn / port exhaustion
                    // in issue #608. Only (re)build when there is no channel yet, or when multiple servers
                    // are configured and we rotate to a different one for failover (paced by the caller's
                    // backoff). A single-server setup therefore reuses one channel for the whole process.
                    if (_channel == null || servers.Length > 1)
                    {
                        await DisposeChannelAsync();
                        EnsureServerAddress();
                        //https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.0#call-insecure-grpc-services-with-net-core-client-2
                        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                        _channel = GrpcChannel.ForAddress(_server, new GrpcChannelOptions { DisposeHttpClient = true });
                    }

#if NET6_0_OR_GREATER
                    // GrpcChannel.ForAddress is lazy: it never opens a socket. Probe connectivity on the
                    // (reused) channel, bounded by ConnectTimeout, so an unreachable server is reported as a
                    // failure (circuit open) instead of a false "Connected". GrpcChannel.ConnectAsync only
                    // exists on the net6.0+ assets of Grpc.Net.Client (gated by SUPPORT_LOAD_BALANCING); the
                    // netstandard2.0 build optimistically marks Connected and lets the next RPC validate --
                    // the channel is still reused, so there is no churn.
                    // Cancel the probe on either the configured ConnectTimeout or agent shutdown, so a long
                    // ConnectTimeout cannot stall graceful shutdown.
                    var connectTimeout = _config.ConnectTimeout > 0 ? _config.ConnectTimeout : DefaultConnectTimeoutMs;
                    using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                    {
                        cts.CancelAfter(connectTimeout);
                        await _channel.ConnectAsync(cts.Token);
                    }
#endif

                    _state = ConnectionState.Connected;
                    _logger.Information($"Connected server[{_channel.Target}].");
                }
                catch (OperationCanceledException ex)
                {
                    // Keep the channel for reuse; only the circuit opens. Skip the noisy log when the
                    // cancellation is agent shutdown rather than a connect timeout.
                    _state = ConnectionState.Failure;
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        _logger.Error("Connect server timeout.", ex);
                    }
                }
                catch (Exception ex)
                {
                    _state = ConnectionState.Failure;
                    _logger.Error("Connect server fail.", ex);
                }
            }
        }

        public async Task ShutdownAsync()
        {
            using (await _lock.LockAsync())
            {
                await DisposeChannelAsync();
                _state = ConnectionState.Failure;
            }
        }

        // Disposes the current channel (and its HttpClient via DisposeHttpClient=true). Callers must hold
        // _lock; it is invoked both from ConnectAsync (server rotation) and ShutdownAsync (agent stop).
        private async Task DisposeChannelAsync()
        {
            var channel = _channel;
            if (channel == null)
            {
                return;
            }

            _channel = null;
            try
            {
                await channel.ShutdownAsync();
                _logger.Information($"Shutdown connection[{channel.Target}].");
            }
            catch (Exception e)
            {
                _logger.Error("Shutdown connection fail.", e);
            }
            finally
            {
                channel.Dispose();
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