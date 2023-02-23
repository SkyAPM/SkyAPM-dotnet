using SkyApm.Tracing;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace SkyApm.PeerFormatters.SqlClient
{
    internal class SqlClientPeerFormatter : IDbPeerFormatter
    {
        private readonly Regex _conStrRegex = new Regex(@"Data Source=(?:([^;,]+?)(?:,(\d+))?;|([^,]+?)(?:,(\d+))?$)", RegexOptions.IgnoreCase);

        public bool Match(DbConnection connection)
        {
            var fullName = connection.GetType().FullName;
            return fullName == "System.Data.SqlClient.SqlConnection" || fullName == "Microsoft.Data.SqlClient.SqlConnection";
        }

        public string GetPeer(DbConnection connection)
        {
            if (connection.ConnectionString == null) return connection.DataSource;

            var match = _conStrRegex.Match(connection.ConnectionString);

            if (match.Success && match.Groups.Count == 5)
            {
                if (match.Groups[1].Success)
                {
                    if (match.Groups[2].Success)
                    {
                        return $"{match.Groups[1].Value}:{match.Groups[2].Value}";
                    }
                    return match.Groups[1].Value + ":1433";
                }
                if (match.Groups[3].Success)
                {
                    if (match.Groups[4].Success)
                    {
                        return $"{match.Groups[3].Value}:{match.Groups[4].Value}";
                    }
                    return match.Groups[3].Value + ":1433";
                }
            }

            return connection.DataSource;
        }
    }
}
