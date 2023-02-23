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
