using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SkyWalking.Extensions.Configuration
{
    internal static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddSkyWalkingDefaultConfig(this IConfigurationBuilder builder)
        {
            var defaultLogFile = Path.Combine("logs", "SkyWalking-{Date}.log");
            var defaultConfig = new Dictionary<string, string>
            {
                {"SkyWalking:Namespace", string.Empty},
                {"SkyWalking:ApplicationCode", "My_Application"},
                {"SkyWalking:SpanLimitPerSegment", "300"},
                {"SkyWalking:Sampling:SamplePer3Secs", "-1"},
                {"SkyWalking:Logging:Level", "Info"},
                {"SkyWalking:Logging:FilePath", defaultLogFile},
                {"SkyWalking:Transport:Interval", "3000"},
                {"SkyWalking:Transport:PendingSegmentLimit", "30000"},
                {"SkyWalking:Transport:PendingSegmentTimeout", "1000"},
                {"SkyWalking:Transport:gRPC:Servers", "localhost:11800"},
                {"SkyWalking:Transport:gRPC:Timeout", "2000"},
                {"SkyWalking:Transport:gRPC:ConnectTimeout", "10000"}
            };
            return builder.AddInMemoryCollection(defaultConfig);
        }
    }
}