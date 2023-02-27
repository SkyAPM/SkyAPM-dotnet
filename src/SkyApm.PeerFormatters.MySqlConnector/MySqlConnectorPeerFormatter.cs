/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

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
