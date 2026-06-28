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
using SkyApm.Logging;

namespace SkyApm.Transport.Grpc
{
    public class ConnectService: ExecutionService
    {
        // The timer ticks at BasePeriod; while the server is unreachable, reconnect attempts back off
        // exponentially up to MaxBackoff so a long outage doesn't rebuild a channel every tick (issue #608).
        private static readonly TimeSpan BasePeriod = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan MaxBackoff = TimeSpan.FromMinutes(5);

        private readonly ConnectionManager _connectionManager;

        // _running guards against re-entrancy: the base timer is periodic and its callback is async void,
        // so it re-fires every BasePeriod even while a previous connect attempt is still awaiting (a probe
        // can take up to ConnectTimeout). Without this guard, attempts would stack up and rebuild a channel
        // per tick, defeating the backoff (issue #608). Because it serialises ExecuteAsync, _backoff and
        // _nextAttemptUtc are only ever touched by a single thread and need no further synchronisation.
        private int _running;
        private TimeSpan _backoff = BasePeriod;
        private DateTime _nextAttemptUtc = DateTime.MinValue;

        public ConnectService(ConnectionManager connectionManager,
            IRuntimeEnvironment runtimeEnvironment,
            ILoggerFactory loggerFactory) : base(runtimeEnvironment, loggerFactory)
        {
            _connectionManager = connectionManager;
        }

        protected override TimeSpan DueTime { get; } = TimeSpan.Zero;
        protected override TimeSpan Period { get; } = BasePeriod;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            {
                // A previous attempt is still in flight; skip this tick.
                return;
            }

            try
            {
                if (DateTime.UtcNow < _nextAttemptUtc)
                {
                    // Still inside the backoff window: skip until it elapses.
                    return;
                }

                if (!_connectionManager.Ready)
                {
                    await _connectionManager.ConnectAsync(cancellationToken);
                }

                if (_connectionManager.Ready)
                {
                    // Connected: reset backoff so the next outage reconnects immediately.
                    _backoff = BasePeriod;
                    _nextAttemptUtc = DateTime.MinValue;
                }
                else
                {
                    // Still unreachable: defer the next attempt by the current backoff,
                    // then grow it (capped at MaxBackoff) for the attempt after.
                    _nextAttemptUtc = DateTime.UtcNow.Add(_backoff);
                    var doubled = TimeSpan.FromTicks(_backoff.Ticks * 2);
                    _backoff = doubled < MaxBackoff ? doubled : MaxBackoff;
                }
            }
            finally
            {
                Interlocked.Exchange(ref _running, 0);
            }
        }

        protected override bool CanExecute() => !_connectionManager.Ready;

        // Dispose the reused channel when the agent shuts down.
        protected override Task Stopping(CancellationToken cancellationToken)
            => _connectionManager.ShutdownAsync();
    }
}