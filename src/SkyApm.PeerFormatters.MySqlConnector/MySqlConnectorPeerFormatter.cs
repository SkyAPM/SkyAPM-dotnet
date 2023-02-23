using SkyApm.Tracing;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace SkyApm.PeerFormatters.MySqlConnector
{
    internal class MySqlConnectorPeerFormatter : IDbPeerFormatter
    {
        private readonly Regex _serverRegex = new Regex(@"server=(?:([^;]+?);|([^;]+?)$)", RegexOptions.IgnoreCase);
        private readonly Regex _portRegex = new Regex(@"port=(\d+)");

        public bool Match(DbConnection connection)
        {
            var fullName = connection.GetType().FullName;
            return fullName == "MySql.Data.MySqlClient.MySqlConnection" || fullName == "MySqlConnector.MySqlConnection";
        }

        public string GetPeer(DbConnection connection)
        {
            if (connection.ConnectionString == null) return connection.DataSource;

            var serverMatch = _serverRegex.Match(connection.ConnectionString);
            var portMatch = _portRegex.Match(connection.ConnectionString);

            var port = portMatch.Success ? portMatch.Groups[1].Value : "3306";

            if (serverMatch.Success && serverMatch.Groups.Count == 3)
            {
                if (serverMatch.Groups[1].Success)
                {
                    return $"{serverMatch.Groups[1].Value}:{port}";
                }
                if (serverMatch.Groups[2].Success)
                {
                    return $"{serverMatch.Groups[2].Value}:{port}";
                }
            }

            return connection.DataSource;
        }
    }
}
