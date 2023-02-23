using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SkyApm.Tracing;
using SkyApm.Utilities.DependencyInjection;
using System;

namespace SkyApm.PeerFormatters.MySqlConnector
{
    public static class MySqlConnectorPeerFormatterExtensions
    {
        public static SkyApmExtensions AddMySqlConnectorPeerFormatter(this SkyApmExtensions extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            extensions.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDbPeerFormatter, MySqlConnectorPeerFormatter>());

            return extensions;
        }
    }
}
