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
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using SkyWalking.DotNet.CLI.Extensions;
using SkyWalking.DotNet.CLI.Utils;

// ReSharper disable IdentifierTypo

// ReSharper disable StringLiteralTypo

namespace SkyWalking.DotNet.CLI.Command
{
    public class InstallCommand : IAppCommand
    {
        private const string git_hosting_startup = "https://github.com/OpenSkywalking/skywalking-netcore-hosting-startup.git";
        private const string manifest_proj = "SkyWalking.Runtime.Store.csproj";

        private readonly DirectoryProvider _directoryProvider;
        private readonly ShellProcessFactory _processFactory;
        private readonly PlatformInformationArbiter _platformInformation;

        public InstallCommand(DirectoryProvider directoryProvider, ShellProcessFactory processFactory, PlatformInformationArbiter platformInformation)
        {
            _directoryProvider = directoryProvider;
            _processFactory = processFactory;
            _platformInformation = platformInformation;
        }

        public string Name { get; } = "install";

        public void Execute(CommandLineApplication command)
        {
            command.Description = "Install SkyWalking .NET Core Agent";
            command.HelpOption();

            var upgradeOption = command.Option("-u|--upgrade", "Upgrade SkyWalking .NET Core Agent", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                if (upgradeOption.HasValue())
                {
                    ConsoleUtils.WriteLine("Upgrading SkyWalking .NET Core Agent.", ConsoleColor.Green);
                }
                else
                {
                    ConsoleUtils.WriteLine("Installing SkyWalking .NET Core Agent.", ConsoleColor.Green);
                }

                Console.WriteLine();

                var workDir = Path.Combine(_directoryProvider.TmpDirectory, _directoryProvider.AgentPath);

                var workDirInfo = new DirectoryInfo(workDir);

                if (workDirInfo.Exists)
                    workDirInfo.Delete(true);

                workDirInfo.Create();

                Console.WriteLine("Create tmp directory : {0}", workDir);

                var hostingStartupDir = Path.Combine(workDir, "hosting_startup");

                var shell = _processFactory.Create(Shell);

                shell.Exec($"git clone {git_hosting_startup} {hostingStartupDir}");

                shell.Exec($"cd {Path.Combine(hostingStartupDir, "manifest")}");

                shell.Exec("dotnet build --configuration Release -nowarn:NU1701");

                shell.Exec($"dotnet store --manifest {manifest_proj} --framework netcoreapp2.1 --runtime {Runtime} -nowarn:NU1701");

                var code = _processFactory.Release(shell);
                if (code != 0)
                {
                    return code;
                }

                var additonalDepsPath = _directoryProvider.GetAdditonalDepsPath(_directoryProvider.AgentPath, "2.1");
                var additonalDepsDirInfo = new DirectoryInfo(additonalDepsPath);
                if (!additonalDepsDirInfo.Exists)
                {
                    additonalDepsDirInfo.Create();
                    Console.WriteLine("Create dotnet additonalDeps directory '{0}'", additonalDepsPath);
                }

                Console.WriteLine();
                ConsoleUtils.WriteLine("You can enable SkyWalking .NET Core Agent using the following command: dotnet sw enable", ConsoleColor.Green);
                ConsoleUtils.WriteLine("SkyWalking .NET Core Agent was successfully installed.", ConsoleColor.Green);

                return 0;
            });
        }

        private string Shell => _platformInformation.GetValue(() => "cmd.exe", () => "sh", () => "bash", () => "sh");

        private string Runtime => _platformInformation.GetValue(() => "win-x64", () => "linux-x64", () => "osx-x64", () => "linux-x64");
    }
}