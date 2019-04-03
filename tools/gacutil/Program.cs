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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace gacutil
{
    class Program
    {
        [DllImport("Fusion.dll", CharSet = CharSet.Auto)]
        internal static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, uint dwReserved);

        /// <summary>
        /// code from https://github.com/Microsoft/referencesource/blob/master/System.Web/Configuration/IAssemblyCache.cs
        /// </summary>
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
        [ComImport]
        internal interface IAssemblyCache
        {
            [MethodImpl(MethodImplOptions.PreserveSig)]
            int UninstallAssembly(
                uint dwFlags,
                [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName,
                IntPtr pvReserved,
                out uint pulDisposition);

            [MethodImpl(MethodImplOptions.PreserveSig)]
            int InstallAssembly(uint dwFlags, 
                [MarshalAs(UnmanagedType.LPWStr)] string pszManifestFilePath,
                IntPtr pvReserved);
        }

        public enum UninstallStatus
        {
            None = 0,
            Uninstalled = 1,
            StillInUse = 2,
            AlreadyUninstalled = 3,
            DeletePending = 4,
            HasInstallReferences = 5,
            ReferenceNotFound = 6
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            try
            {
                var mode = args[0].ToLower();
                CreateAssemblyCache(out var ppAsmCache, 0U);
                switch (mode)
                {
                    case "/i":
                        ppAsmCache.InstallAssembly(0, args[1], (IntPtr)0);
                        Console.WriteLine($"gacutil /i {args[1]}");
                        break;
                    case "/u":
                    {
                        ppAsmCache.UninstallAssembly(0U, args[1], (IntPtr) 0, out var status);
                        Console.WriteLine($"gacutil /u {args[1]}, status:{(UninstallStatus)status}");
                    }
                        break;
                    default:
                        throw new ArgumentException("only support gacutil /i /u assembly");
                }
            }
            catch (Exception e)
            {
                PrintUsage();
                Console.WriteLine(e);
                Environment.Exit(-2);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" gacutil /i /u assembly");
            Console.WriteLine(" /i : Install Assembly(Use Assembly Path) into GAC  ");
            Console.WriteLine(" /u : Uninstall Assembly(Use Assembly Name) from GAC");
        }
    }
}
