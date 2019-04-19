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
using System.Linq;
using System.Reflection;
using SkyApm.Logging;

namespace SkyApm.Diagnostics
{
    internal class TracingDiagnosticObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly IReadOnlyDictionary<string, IEnumerable<TracingDiagnosticMethod>> _methodCollection;
        private readonly ILogger _logger;

        public TracingDiagnosticObserver(ITracingDiagnosticProcessor tracingDiagnosticProcessor,
            ILoggerFactory loggerFactory)
        {
            _methodCollection = (from method in tracingDiagnosticProcessor.GetType().GetMethods()
                                 let diagnosticName = method.GetCustomAttribute<DiagnosticName>()
                                 where diagnosticName != null
                                 group method by diagnosticName.Name into g
                                 select new KeyValuePair<string, IEnumerable<TracingDiagnosticMethod>>(g.Key,
                                     g.Select(m => new TracingDiagnosticMethod(tracingDiagnosticProcessor, m)).ToArray()))
                 .ToDictionary(kv => kv.Key, kv => kv.Value);

            _logger = loggerFactory.CreateLogger(typeof(TracingDiagnosticObserver));
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            if (!_methodCollection.TryGetValue(value.Key, out var methods)) return;

            foreach (var method in methods)
            {
                try
                {
                    method.Invoke(value.Value);
                }
                catch (Exception exception)
                {
                    _logger.Error("Invoke diagnostic method exception.", exception);
                }
            }
        }
    }
}
