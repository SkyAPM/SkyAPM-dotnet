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

using Microsoft.Extensions.Configuration;

// ReSharper disable StringLiteralTypo
namespace SkyApm.Utilities.Configuration;

public class ConfigurationFactory : IConfigurationFactory
{
    private const string CONFIG_FILE_PATH_COMPATIBLE = "SKYWALKING__CONFIG__PATH";
    private const string CONFIG_FILE_PATH = "SKYAPM__CONFIG__PATH";
    private readonly IEnvironmentProvider _environmentProvider;
    private readonly IEnumerable<IAdditionalConfigurationSource> _additionalConfigurations;
    private readonly IConfiguration _configuration;

    public ConfigurationFactory(IEnvironmentProvider environmentProvider,
        IEnumerable<IAdditionalConfigurationSource> additionalConfigurations,
        IConfiguration configuration)
    {
        _environmentProvider = environmentProvider;
        _additionalConfigurations = additionalConfigurations;
        _configuration = configuration;
    }

    public IConfiguration Create()
    {
        var builder = new ConfigurationBuilder();

        _ = builder.AddSkyWalkingDefaultConfig(_configuration);

        _ = builder.AddJsonFile("appsettings.json", true)
            .AddJsonFile($"appsettings.{_environmentProvider.EnvironmentName}.json", true);

        _ = builder.AddJsonFile("skywalking.json", true)
            .AddJsonFile($"skywalking.{_environmentProvider.EnvironmentName}.json", true);

        _ = builder.AddJsonFile("skyapm.json", true)
            .AddJsonFile($"skyapm.{_environmentProvider.EnvironmentName}.json", true);

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH_COMPATIBLE)))
        {
            _ = builder.AddJsonFile(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH_COMPATIBLE), false);
        }

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH)))
        {
            _ = builder.AddJsonFile(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH), false);
        }

        _ = builder.AddEnvironmentVariables();

        if (_configuration != null)
        {
            _ = builder.AddConfiguration(_configuration);
        }

        foreach (var additionalConfiguration in _additionalConfigurations)
        {
            additionalConfiguration.Load(builder);
        }

        return builder.Build();
    }
}