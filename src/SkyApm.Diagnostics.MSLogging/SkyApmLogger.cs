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
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Transport;

namespace SkyApm.Diagnostics.MSLogging
{
    public class SkyApmLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ISkyApmLogDispatcher _skyApmLogDispatcher;
        private readonly ISegmentContextAccessor _segmentContextAccessor;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly TracingConfig _tracingConfig;
        private readonly DiagnosticsLoggingConfig _logCollectorConfig;

        public SkyApmLogger(string categoryName, ISkyApmLogDispatcher skyApmLogDispatcher,
            ISegmentContextAccessor segmentContextAccessor,
            IEntrySegmentContextAccessor entrySegmentContextAccessor,
            IConfigAccessor configAccessor)
        {
            _categoryName = categoryName;
            _skyApmLogDispatcher = skyApmLogDispatcher;
            _segmentContextAccessor = segmentContextAccessor;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _logCollectorConfig = configAccessor.Get<DiagnosticsLoggingConfig>();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var tags = new Dictionary<string, object>
            {
                { "logger", _categoryName },
                { "level", logLevel },
                { "thread", Thread.CurrentThread.ManagedThreadId },
            };
            if (exception != null)
            {
                tags["errorType"] = exception.GetType().ToString();
            }
            var message = state.ToString();
            if (exception != null)
            {
                message += "\r\n" + (exception.HasInnerExceptions() ? exception.ToDemystifiedString(_tracingConfig.ExceptionMaxDepth) : exception.ToString());
            }
            SegmentContext segmentContext = _segmentContextAccessor.Context;
            var logContext = new LogRequest()
            {
                Message = message ?? string.Empty,
                Tags = tags,
                SegmentReference = segmentContext == null
                    ? null
                    : new LogSegmentReference()
                    {
                        TraceId = segmentContext.TraceId,
                        SegmentId = segmentContext.SegmentId,
                    },
            };
            if (_entrySegmentContextAccessor.Context != null)
            {
                logContext.Endpoint = _entrySegmentContextAccessor.Context.Span.OperationName.ToString();
            }
            _skyApmLogDispatcher.Dispatch(logContext);
        }

        public bool IsEnabled(LogLevel logLevel) => (int)logLevel >= (int)_logCollectorConfig.CollectLevel;

        public IDisposable BeginScope<TState>(TState state) => default!;
    }
}