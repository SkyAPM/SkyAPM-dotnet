using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using SkyApm.Common;

namespace SkyApm.Diagnostics.EntityFrameworkCore
{
    public class MySqlConnectorSpanMetadataProvider : IEntityFrameworkCoreSpanMetadataProvider
    {
        public StringOrIntValue Component { get; } = Components.POMELO_ENTITYFRAMEWORKCORE_MYSQL;

        public bool Match(DbConnection connection)
        {
            return connection.GetType().FullName == "MySqlConnector.MySqlConnection";
        }

        public string GetPeer(DbConnection connection)
        {
            return connection.DataSource;
        }
    }
}
