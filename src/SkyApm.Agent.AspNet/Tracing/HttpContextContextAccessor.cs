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
using System.Web;

namespace SkyApm.AspNet.Tracing
{
    /// <summary>
    /// Sorry for the idiotic name. It's suppose to be "Accessor of the <see cref="SegmentContext"/> provided via <see cref="HttpContext"/>".
    /// </summary>
    public abstract class HttpContextContextAccessor<T> where T : class
    {
        public virtual SegmentContext Context
        {
            get => GetValueOrNull();
            set => SetValue(value);
        }

        private SegmentContext GetValueOrNull()
        {
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains(typeof(T)))
            {
                return HttpContext.Current.Items[typeof(T)] as SegmentContext;
            }
            return null;
        }

        private void SetValue(SegmentContext value)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items[typeof(T)] = value;
            }
        }
    }
}
