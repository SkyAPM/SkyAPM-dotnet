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
using SkyApm.Tracing.Segments;
using SkyApm.Common;
using SkyApm.Config;

namespace SkyApm.Tracing
{
    public static class SegmentSpanExtensions
    {
        [Obsolete("Use the overload method with the TracingConfig parameter, if pass in the exception")]
        public static void ErrorOccurred(this SegmentSpan span, Exception exception = null)
        {
            span.ErrorOccurred(exception, null);
        }

        public static void ErrorOccurred(this SegmentSpan span)
        {
            if (span == null)
                return;

            span.IsError = true;
        }

        public static void ErrorOccurred(this SegmentSpan span, Exception exception, TracingConfig tracingConfig)
        {
            if (span == null)
                return;

            span.IsError = true;

            if (exception == null)
                return;

            if (tracingConfig == null)
                tracingConfig = new TracingConfig();

            var stackTrace = exception.HasInnerExceptions() ? exception.ToDemystifiedString(tracingConfig.ExceptionMaxDepth) : exception.StackTrace;
            span.AddLog(LogEvent.Event("error"),
                LogEvent.ErrorKind(exception.GetType().FullName),
                LogEvent.Message(exception.Message),
                LogEvent.ErrorStack(stackTrace));
        }
    }
}