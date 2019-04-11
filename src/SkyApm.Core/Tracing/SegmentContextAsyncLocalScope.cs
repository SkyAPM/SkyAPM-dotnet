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

using SkyApm.Tracing.Segments;

namespace SkyApm.Tracing
{
    internal class SegmentContextAsyncLocalScope : ISegmentContextScope
    {
        private readonly SegmentContextScopeManager _scopeManager;
        private readonly ISegmentContextScope _scopeToRestore;
        private bool _released;

        public SegmentContextAsyncLocalScope(SegmentContextScopeManager scopeManager,
            SegmentContext segmentContext)
        {
            _scopeManager = scopeManager;
            SegmentContext = segmentContext;
            _scopeToRestore = scopeManager.Active;
            scopeManager.Active = this;
        }

        public SegmentContext SegmentContext { get; }

        public void Release()
        {
            if (_released)
            {
                return;
            }
            _scopeManager.Active = _scopeToRestore;
            _released = true;
        }
    }
}
