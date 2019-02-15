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

namespace SkyApm.DotNet.CLI.Utils
{
    public static class ConsoleUtils
    {
        public static void WriteLine(string message, ConsoleColor foregroundColor)
        {
            var currentForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(message);
            Console.ForegroundColor = currentForegroundColor;
        }

        public static void WriteWelcome()
        {
            Console.WriteLine("*********************************************************************");
            Console.WriteLine("*                                                                   *");
            Console.WriteLine("*                   Welcome to Apache SkyWalking                    *");
            Console.WriteLine("*                                                                   *");
            Console.WriteLine("*********************************************************************");
        }
    }
}