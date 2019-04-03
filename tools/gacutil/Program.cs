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
using System.EnterpriseServices.Internal;

namespace gacutil
{
    class Program
    {
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
                var publish = new Publish();
                switch (mode)
                {
                    case "/i":
                        publish.GacInstall(args[1]);
                        Console.WriteLine($"gacutil /i {args[1]}");
                        break;
                    case "/u":
                    {
                        publish.GacRemove(args[1]);  // maybe fail , no return uninstall status.
                        Console.WriteLine($"gacutil /u {args[1]}");
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
