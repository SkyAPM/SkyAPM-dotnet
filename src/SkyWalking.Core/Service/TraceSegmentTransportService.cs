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
using SkyWalking.Config;
using SkyWalking.Context;
using SkyWalking.Context.Trace;
using SkyWalking.Logging;
using SkyWalking.Transport;

namespace SkyWalking.Service
{
    public class TraceSegmentTransportService : InstrumentationService, ITracingContextListener
    {
        private readonly TransportConfig _config;
        private readonly ITraceDispatcher _dispatcher;

        public TraceSegmentTransportService(IConfigAccessor configAccessor, ITraceDispatcher dispatcher,
            IInstrumentationClient instrumentation, IRuntimeEnvironment runtimeEnvironment, IInstrumentationLoggerFactory loggerFactory)
            : base(instrumentation, runtimeEnvironment, loggerFactory)
        {
            _dispatcher = dispatcher;
            _config = configAccessor.Get<TransportConfig>();
            Period = TimeSpan.FromSeconds(_config.Interval);
        }

        protected override TimeSpan DueTime { get; } = TimeSpan.FromSeconds(5);

        protected override TimeSpan Period { get; }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _dispatcher.Flush(cancellationToken);
        }

        public void AfterFinished(ITraceSegment traceSegment)
        {
            if (!traceSegment.IsIgnore)
                _dispatcher.Dispatch(traceSegment.Transform());
        }
    }
}