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

using Microsoft.Extensions.Hosting;
using SkyApm.Config;
using SkyApm.Tracing;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Utilities.StaticAccessor
{
    internal class StaticAccessorHostedService : IHostedService
    {
        private readonly ITracingContext _tracingContext;
        private readonly IConfigAccessor _configAccessor;

        public StaticAccessorHostedService(ITracingContext tracingContext, IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _configAccessor = configAccessor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            SkyApmInstances.TracingContext = _tracingContext;
            SkyApmInstances.ConfigAccessor = _configAccessor;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
