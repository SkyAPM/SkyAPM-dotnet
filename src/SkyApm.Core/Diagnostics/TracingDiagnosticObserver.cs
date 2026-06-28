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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SkyApm.Logging;

namespace SkyApm.Diagnostics
{
    internal class TracingDiagnosticObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly Dictionary<string, TracingDiagnosticMethod> _methodCollection;
        private readonly ConcurrentDictionary<string, byte> _disabledDiagnostics = new ConcurrentDictionary<string, byte>();
        private readonly ILogger _logger;

        public TracingDiagnosticObserver(ITracingDiagnosticProcessor tracingDiagnosticProcessor,
            ILoggerFactory loggerFactory)
        {
            _methodCollection = new TracingDiagnosticMethodCollection(tracingDiagnosticProcessor)
                .ToDictionary(method => method.DiagnosticName);
            _logger = loggerFactory.CreateLogger(typeof(TracingDiagnosticObserver));
        }

        public bool IsEnabled(string diagnosticName)
        {
            return _methodCollection.ContainsKey(diagnosticName);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            if (!_methodCollection.TryGetValue(value.Key, out var method))
                return;

            if (_disabledDiagnostics.ContainsKey(value.Key))
                return;

            try
            {
                method.Invoke(value.Key, value.Value);
            }
            catch (Exception exception) when (exception is MissingMemberException || exception is TypeLoadException)
            {
                // A binding/structural error means the instrumented library's version is incompatible
                // with this plugin (e.g. a diagnostic event type or property changed across versions).
                // It fails deterministically for every event, so disable this handler after logging once
                // instead of spamming the same error on every call (issue #565).
                if (_disabledDiagnostics.TryAdd(value.Key, 0))
                {
                    _logger.Warning($"Disabled diagnostic handler '{value.Key}' due to an incompatible " +
                                    "instrumented library version; ensure the installed package version matches " +
                                    $"this SkyAPM plugin. {exception.GetType().Name}: {exception.Message}");
                }
            }
            catch (Exception exception)
            {
                _logger.Error("Invoke diagnostic method exception.", exception);
            }
        }
    }
}