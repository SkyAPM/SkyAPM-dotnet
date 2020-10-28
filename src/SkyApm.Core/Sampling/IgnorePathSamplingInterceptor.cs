using SkyApm.Config;
using SkyApm.Tracing;
using SkyWalking.Common;
using System;
using System.Collections.Generic;
using System.Text;

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
