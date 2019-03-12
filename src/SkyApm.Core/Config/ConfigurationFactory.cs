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

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace SkyApm.Config
{
    public class ConfigurationFactory : IConfigurationFactory
    {
        private const string CONFIG_FILE_PATH = "SKYAPM__CONFIG__PATH";

        public IConfiguration Create()
        {
            var builder = new ConfigurationBuilder();

            builder.AddSkyWalkingDefaultConfig();

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH)))
            {
                builder.AddJsonFile(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH), false);
            }

            builder.AddEnvironmentVariables();

            return builder.Build();
        }
    }
}