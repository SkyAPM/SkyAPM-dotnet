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

using Grpc.Core;
using SkyApm.Tracing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyApm.Diagnostics.Grpc
{
    public class GrpcCarrierHeaderCollection : ICarrierHeaderCollection
    {
        private readonly Metadata _metadata;

        public GrpcCarrierHeaderCollection(Metadata metadata)
        {
            _metadata = metadata ?? new Metadata();
        }

        public void Add(string key, string value)
        {
            _metadata.Add(new Metadata.Entry(key, value));
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _metadata.Select(m => new KeyValuePair<string, string>(m.Key, m.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _metadata.GetEnumerator();
        }
    }
}
