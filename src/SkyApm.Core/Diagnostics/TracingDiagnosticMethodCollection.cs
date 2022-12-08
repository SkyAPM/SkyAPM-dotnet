﻿/*
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

using System.Collections;
using System.Reflection;

namespace SkyApm.Diagnostics;

internal class TracingDiagnosticMethodCollection : IEnumerable<TracingDiagnosticMethod>
{
    private readonly List<TracingDiagnosticMethod> _methods;

    public TracingDiagnosticMethodCollection(ITracingDiagnosticProcessor tracingDiagnosticProcessor)
    {
        _methods = new();
        foreach (var method in tracingDiagnosticProcessor.GetType().GetMethods())
        {
            var diagnosticName = method.GetCustomAttribute<DiagnosticNameAttribute>();
            if(diagnosticName==null)
                continue;
            _methods.Add(new(tracingDiagnosticProcessor, method, diagnosticName.Name));
        }
    }
        
    public IEnumerator<TracingDiagnosticMethod> GetEnumerator()
    {
        return _methods.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _methods.GetEnumerator();
    }
}