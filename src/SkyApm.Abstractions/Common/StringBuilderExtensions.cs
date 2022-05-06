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
    internal static class StringBuilderExtensions
    {
        public static StringBuilder Append(this StringBuilder builder, Exception exception, int maxDepth)
        {
            return builder.Append(exception, 0, maxDepth);
        }

        private static StringBuilder Append(this StringBuilder builder, Exception exception, int currentDepth, int maxDepth)
        {
            try
            {
                builder.Append(exception.GetType());
                if (!string.IsNullOrEmpty(exception.Message))
                {
                    builder.Append(": ").Append(exception.Message);
                }

                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    builder.AppendLine();
                    builder.Append(exception.StackTrace);
                }

                if (++currentDepth >= maxDepth && maxDepth > 0)
                    return builder;

                if (exception is AggregateException aggEx)
                {
                    foreach (var ex in aggEx.InnerExceptions)
                    {
                        builder.AppendInnerException(ex, currentDepth, maxDepth);
                        if (++currentDepth >= maxDepth && maxDepth > 0)
                            return builder;
                    }
                }
                else if (exception.InnerException != null)
                {
                    builder.AppendInnerException(exception.InnerException, currentDepth, maxDepth);
                }
            }
            catch
            {
                // Processing exceptions shouldn't throw exceptions
            }

            return builder;
        }

        private static void AppendInnerException(this StringBuilder builder, Exception exception, int currentDepth, int maxDepth)
            => builder.AppendLine()
                .Append(" ---> ")
                .Append(exception, currentDepth, maxDepth)
                .AppendLine()
                .Append("   --- End of inner exception stack trace ---");
    }
}
