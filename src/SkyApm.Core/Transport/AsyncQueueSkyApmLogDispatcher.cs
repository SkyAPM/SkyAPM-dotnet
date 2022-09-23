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

using SkyApm.Config;
using SkyApm.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SkyApm.Transport
{
    public class AsyncQueueSkyApmLogDispatcher : ISkyApmLogDispatcher,IDisposable
    {
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancellation;

        private readonly Channel<LoggerRequest> _segmentChannel;

        private readonly IRuntimeEnvironment _runtimeEnvironment;

        private readonly ILoggerReporter _loggerReporter;
        
        private readonly TransportConfig _config;


        public AsyncQueueSkyApmLogDispatcher(IConfigAccessor configAccessor, ILoggerFactory loggerFactory, ILoggerReporter loggerReporter, IRuntimeEnvironment runtimeEnvironment)
        {
            _logger = loggerFactory.CreateLogger(typeof(AsyncQueueSkyApmLogDispatcher));
            _config = configAccessor.Get<TransportConfig>();
            _runtimeEnvironment = runtimeEnvironment;
            _segmentChannel = Channel.CreateBounded<LoggerRequest>(new BoundedChannelOptions(7) { FullMode = BoundedChannelFullMode.DropWrite });
            _cancellation = new CancellationTokenSource();
            _loggerReporter = loggerReporter;
            Task.Factory.StartNew(Loop, _cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public bool Dispatch(LoggerRequest loggerRequest)
        {
            if (!_runtimeEnvironment.Initialized || loggerRequest == null)
                return false;
            if (!_segmentChannel.Writer.TryWrite(loggerRequest))
            {
                return false;
            }

            _logger.Debug($"Dispatch trace segment. [SegmentId]={loggerRequest.SegmentReference?.SegmentId}.");
            return true;
        }

        private async Task Loop()
        {
            var loggers = new List<LoggerRequest>(_config.BatchSize);
            while (!_cancellation.IsCancellationRequested)
            {
                if (!await _segmentChannel.Reader.WaitToReadAsync(_cancellation.Token))
                {
                    break;
                }

                var item = await _segmentChannel.Reader.ReadAsync(_cancellation.Token);
                loggers.Add(item);
                if (loggers.Count >= _config.BatchSize)
                {
                    var tmp = new List<LoggerRequest>(loggers);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    _loggerReporter.ReportAsync(tmp, _cancellation.Token);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    loggers.Clear();
                }
            }
            //stop writing
            _segmentChannel.Writer.Complete();

            if (loggers.Any())
            {

#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                _loggerReporter.ReportAsync(loggers, _cancellation.Token);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            }
        }

        public void Dispose()
        {
            _cancellation.Cancel();
        }
    }
}
