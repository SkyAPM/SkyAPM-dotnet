using Microsoft.Extensions.Configuration;
using SkyApm.Utilities.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyApm.Agent.AspNet.Configuration
{
    public class ConfigurationFactory : SkyApm.Utilities.Configuration.ConfigurationFactory
    {
        public ConfigurationFactory(IEnvironmentProvider environmentProvider, IEnumerable<IAdditionalConfigurationSource> additionalConfigurations) : base(environmentProvider, additionalConfigurations, null)
        {
        }
    }
}
