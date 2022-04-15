using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace SkyApm.Core.Logging
{
    public class SkyApmLoggerOptionsSetup : ConfigureFromConfigurationOptions<FileLoggerOptions>
    {
        public SkyApmLoggerOptionsSetup(ILoggerProviderConfiguration<SkyApmLoggerOptionsSetup> providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }

    }


    public class FileLoggerOptions
    {
        string fFolder;
        int fMaxFileSizeInMB;
        int fRetainPolicyFileCount;

        public FileLoggerOptions()
        {
        }

        public LogLevel LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Information;

        public string Folder
        {
            get
            {
                return !string.IsNullOrWhiteSpace(fFolder) ?
                fFolder : System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);
            }
            set { fFolder = value; }
        }

        public int MaxFileSizeInMB
        {
            get { return fMaxFileSizeInMB > 0 ? fMaxFileSizeInMB : 2; }
            set { fMaxFileSizeInMB = value; }
        }

        public int RetainPolicyFileCount
        {
            get { return fRetainPolicyFileCount < 5 ? 5 : fRetainPolicyFileCount; }
            set { fRetainPolicyFileCount = value; }
        }
    }
}
