using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SkyApm.Tracing;
using SkyApm.Utilities.DependencyInjection;
using System;

namespace SkyApm.PeerFormatters.SqlClient
{
    public static class SqlClientPeerFormatterExtensions
    {
        public static SkyApmExtensions AddSqlClientPeerFormatter(this SkyApmExtensions extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            extensions.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDbPeerFormatter, SqlClientPeerFormatter>());

            return extensions;
        }
    }
}
