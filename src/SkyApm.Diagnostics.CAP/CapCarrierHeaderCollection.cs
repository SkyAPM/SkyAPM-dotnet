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

using System.Collections;
using System.Collections.Generic;
using DotNetCore.CAP.Diagnostics;
using SkyApm.Tracing;

namespace SkyApm.Diagnostics.CAP
{
    public class CapCarrierHeaderCollection : ICarrierHeaderCollection
    {
        private readonly TracingHeaders _tracingHeaders;

        public CapCarrierHeaderCollection(TracingHeaders tracingHeaders)
        {
            _tracingHeaders = tracingHeaders;
        }
        
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _tracingHeaders.GetEnumerator();
        }

        public void Add(string key, string value)
        {
            _tracingHeaders.Add(key, value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tracingHeaders.GetEnumerator();
        }
    }
}