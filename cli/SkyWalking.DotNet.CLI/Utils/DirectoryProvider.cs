/*
 * Licensed to the OpenSkywalking under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
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
using System.IO;
// ReSharper disable IdentifierTypo
// ReSharper disable MemberCanBePrivate.Global

namespace SkyWalking.DotNet.CLI.Utils
{
    public class DirectoryProvider
    {
        private readonly PlatformInformationArbiter _platformInformation;

        public string TmpDirectory => _platformInformation.GetValue(
            () => "C:\\tmp",
            () => "/tmp",
            () => "/tmp",
            () => "/tmp");

        public string UserDirectory => _platformInformation.GetValue(
            () => Environment.GetEnvironmentVariable("USERPROFILE"),
            () => Environment.GetEnvironmentVariable("HOME"),
            () => Environment.GetEnvironmentVariable("HOME"),
            () => "~");

        public string DotnetDirectory => Path.Combine(UserDirectory, ".dotnet");

        public string AgentPath => "skywalking.agent.aspnetcore";
        
        public string AdditonalDepsRootDirectory => _platformInformation.GetValue(
            () => Environment.GetEnvironmentVariable("PROGRAMFILES"),
            () => "/usr/local/share",
            () => "/usr/local/share",
            () => "/usr/local/share");

        public DirectoryProvider(PlatformInformationArbiter platformInformation)
        {
            _platformInformation = platformInformation;
        }

        public string GetAdditonalDepsPath(string additonalName, string frameworkVersion)
        {
            return Path.Combine(GetAdditonalDepsDirectory(additonalName), "shared", "Microsoft.NETCore.App", frameworkVersion);
        }
        
        public string GetAdditonalDepsDirectory(string additonalName)
        {
            return Path.Combine(AdditonalDepsRootDirectory, additonalName);
        }
    }
}
