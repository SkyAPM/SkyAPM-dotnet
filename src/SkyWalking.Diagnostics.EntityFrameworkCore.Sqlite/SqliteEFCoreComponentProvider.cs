using System.Data.Common;
using Microsoft.Data.Sqlite;
using SkyWalking.NetworkProtocol.Trace;

namespace SkyWalking.Diagnostics.EntityFrameworkCore
{
    public class SqliteEFCoreSpanMetadataProvider : IEfCoreSpanMetadataProvider
    {
        public IComponent Component { get; } = ComponentsDefine.EntityFrameworkCore_Sqlite;
        
        public bool Match(DbConnection connection)
        {
            return connection is SqliteConnection;
        }

        public string GetPeer(DbConnection connection)
        {
            string dataSource;
            switch (connection.DataSource)
            {
                    case "":
                        dataSource = "localhost";
                        break;
                    default:
                        dataSource = connection.DataSource;
                        break;
            }

            return $"{dataSource}";
        }
    }
}