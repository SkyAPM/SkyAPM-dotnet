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

using Microsoft.Extensions.Logging;
using SkyApm.Tracing;
using SkyApm.Transport;
using System.Collections.Concurrent;

namespace SkyApm.Diagnostics.MSLogging;

public class SkyApmLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, SkyApmLogger> _doveLoggers = new();
    private readonly ISkyApmLogDispatcher _skyApmLogDispatcher;
    private readonly ISegmentContextAccessor _segmentContextAccessor;

    public SkyApmLoggerProvider(ISkyApmLogDispatcher skyApmLogDispatcher, ISegmentContextAccessor segmentContextAccessor)
    {
        _skyApmLogDispatcher = skyApmLogDispatcher;
        _segmentContextAccessor = segmentContextAccessor;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _doveLoggers.GetOrAdd(categoryName, _ => new(categoryName, _skyApmLogDispatcher, _segmentContextAccessor));
    }
}