using SkyWalking.Config;

namespace SkyWalking.Extensions.Logging
{
    [Config("SkyWalking", "Logging")]
    public class LoggingConfig
    {
        public string Level { get; set; }
        
        public string FilePath { get; set; }
    }
}