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
using SkyApm.Tracing;
using SkyApm.Common;
using System.Collections.Generic;

namespace SkyApm.Sampling
{
    public class IgnorePathSamplingInterceptor : ISamplingInterceptor
    {
        private readonly bool _sample_on;
        private readonly List<string> _ignorePaths;

        public int Priority => int.MinValue + 998;

        public IgnorePathSamplingInterceptor(IConfigAccessor configAccessor)
        {
            var ignorePaths = configAccessor.Get<SamplingConfig>().IgnorePaths;
            _sample_on = ignorePaths?.Count > 0;
            if(_sample_on)
            {
                _ignorePaths = ignorePaths;
            }
        }

        public bool Invoke(SamplingContext samplingContext, Sampler next)
        {
            if(!_sample_on) return next(samplingContext);

            foreach (var pattern in _ignorePaths)
            {
                if (FastPathMatcher.Match(pattern, samplingContext.OperationName))
                    return false;
            }

            return next(samplingContext);
        }
    }
}
