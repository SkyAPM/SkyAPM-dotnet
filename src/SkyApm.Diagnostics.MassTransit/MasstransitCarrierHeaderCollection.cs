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

using MassTransit;
using SkyApm.Tracing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkyApm.Diagnostics.MassTransit
{
    public class MasstransitCarrierHeaderCollection : ICarrierHeaderDictionary
    {
        private readonly Headers _headers;

        public MasstransitCarrierHeaderCollection(Headers headers)
        {
            _headers = headers;
        }
        public void Add(string key, string value)
        {
            //deal with GRPC transport later
            if (_headers.GetType().GetInterfaces().Contains(typeof(SendHeaders)))
            {
                ((SendHeaders)_headers).Set(key, value);
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        public string Get(string key)
        {
            if (_headers.TryGetHeader(key, out var value))
                return value.ToString();
            return null;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _headers.Select(o => new KeyValuePair<string, string>(o.Key, o.Value.ToString())).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
