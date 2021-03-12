using BenchmarkDotNet.Attributes;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Utilities.Configuration;
using System;
using System.Collections;
using System.Linq;

namespace SkyApm.Benchmark
{
    public class UniqueIdGenerate
    {
        private static readonly IUniqueIdGenerator Generator;

        static UniqueIdGenerate()
        {
            var configFactory = new ConfigurationFactory(null, Enumerable.Empty<IAdditionalConfigurationSource>(), null);
            var configAccessor = new ConfigAccessor(configFactory);
            Generator = new UniqueIdGenerator(configAccessor);
        }

        [Benchmark]
        public void Generate() => Generator.Generate();
    }
}
