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
using Microsoft.Extensions.Logging;
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

        public SkyApmLogger(string categoryName, ISkyApmLogDispatcher skyApmLogDispatcher,
            ISegmentContextAccessor segmentContextAccessor)
        {
            _categoryName = categoryName;
            _skyApmLogDispatcher = skyApmLogDispatcher;
            _segmentContextAccessor = segmentContextAccessor;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var logs = new Dictionary<string, object>
            {
                { "className", _categoryName },
                { "Level", logLevel },
                { "logMessage", state.ToString() ?? "" }
            };
            SegmentContext segmentContext = _segmentContextAccessor.Context;
            var logContext = new LoggerRequest()
            {
                Logs = logs,
                SegmentReference = segmentContext == null
                    ? null
                    : new LoggerSegmentReference()
                    {
                        TraceId = segmentContext.TraceId,
                        SegmentId = segmentContext.SegmentId
                    },
            };
            _skyApmLogDispatcher.Dispatch(logContext);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => default!;
    }
}