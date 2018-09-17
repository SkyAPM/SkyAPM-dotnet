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
using System.Threading;
using System.Threading.Tasks;
using SkyWalking.Logging;

namespace SkyWalking.Service
{
    public abstract class InstrumentationService : IInstrumentationService
    {
        private Timer _timer;
        private CancellationTokenSource _cancellationTokenSource;
        
        protected readonly IInstrumentationLogger _logger;
        protected readonly IRuntimeEnvironment _runtimeEnvironment;
        protected readonly IInstrumentationClient _instrumentation;

        protected InstrumentationService(IInstrumentationClient instrumentation, IRuntimeEnvironment runtimeEnvironment, IInstrumentationLoggerFactory loggerFactory)
        {
            _instrumentation = instrumentation;
            _runtimeEnvironment = runtimeEnvironment;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var source = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            _timer = new Timer(Callback, source, DueTime, Period);
            _logger.Debug($"Start {GetType().Name}.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _cancellationTokenSource?.Cancel();
            _logger.Debug($"Stop {GetType().Name}.");
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async void Callback(object state)
        {
            if (state is CancellationTokenSource token && !token.IsCancellationRequested && CanExecute())
            {
                await ExecuteAsync(token.Token);
            }
        }
        
        protected virtual bool CanExecute() => _runtimeEnvironment.Initialized;

        protected abstract TimeSpan DueTime { get; }

        protected abstract TimeSpan Period { get; }

        protected abstract Task ExecuteAsync(CancellationToken cancellationToken);    
    }
}