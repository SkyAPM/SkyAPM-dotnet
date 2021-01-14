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

namespace SkyApm.Common
{
    public static class Components
    {
        public static readonly StringOrIntValue ASPNETCORE= new StringOrIntValue(3001, "AspNetCore");

        public static readonly StringOrIntValue HTTPCLIENT = new StringOrIntValue(2, "HttpClient");
        
        public static readonly StringOrIntValue ENTITYFRAMEWORKCORE = new StringOrIntValue(3002, "EntityFrameworkCore");
        
        public static readonly StringOrIntValue SQLCLIENT = new StringOrIntValue(3003, "SqlClient");
        
        public static readonly StringOrIntValue CAP = new StringOrIntValue(3004, "CAP");
        
        public static readonly StringOrIntValue ENTITYFRAMEWORKCORE_SQLITE = new StringOrIntValue(3011, "EntityFrameworkCore.Sqlite");
        
        public static readonly StringOrIntValue POMELO_ENTITYFRAMEWORKCORE_MYSQL = new StringOrIntValue(3012, "Pomelo.EntityFrameworkCore.MySql");
        
        public static readonly StringOrIntValue NPGSQL_ENTITYFRAMEWORKCORE_POSTGRESQL = new StringOrIntValue(3013, "Npgsql.EntityFrameworkCore.PostgreSQL");
        
        public static readonly StringOrIntValue ASPNET = new StringOrIntValue(3015, "AspNet");

        public static readonly StringOrIntValue SMART_SQL = new StringOrIntValue(3016, "SmartSql");

        public static readonly StringOrIntValue GRPC = new StringOrIntValue(23, "GRPC");

        public static readonly StringOrIntValue MongoDBCLIENT = new StringOrIntValue(42, "MongoDB.Driver");
    }
}
