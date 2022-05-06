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
using System.Text;

namespace SkyApm.Common
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Gets demystified string representation of the <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="maxDepth"></param>
        /// <returns></returns>
        public static string ToDemystifiedString(this Exception exception, int maxDepth = 3)
        {
            var builder = new StringBuilder();
            builder.Append(exception, maxDepth);
            return builder.ToString();
        }

        /// <summary>
        /// Check if the exception has inner exceptions
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static bool HasInnerExceptions(this Exception exception)
        {
            return exception.InnerException != null || exception is AggregateException;
        }
    }
}
