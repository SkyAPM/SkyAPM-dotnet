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

using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Utilities.StaticAccessor;

namespace System.Threading
{
    internal class SkyApmThreadPoolWorkItem : IThreadPoolWorkItem
    {
        private readonly string _operationName;
        private readonly IThreadPoolWorkItem _item;
        private readonly CrossThreadCarrier _carrier;

        public SkyApmThreadPoolWorkItem(IThreadPoolWorkItem item)
        {
            _operationName = item.GetType().FullName;
            var prepare = SkyApmInstances.TracingContext.CreateLocal(_operationName);
            _carrier = prepare.GetCrossThreadCarrier();

            _item = item;

            SkyApmInstances.TracingContext.Finish(prepare);
        }

        public void Execute()
        {
            var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + _operationName, _carrier);
            try
            {
                _item.Execute();
            }
            catch (Exception ex)
            {
                local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                throw ex;
            }
            finally
            {
                SkyApmInstances.TracingContext.Finish(local);
            }
        }
    }
}
