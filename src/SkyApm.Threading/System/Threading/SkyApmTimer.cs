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

using System.Threading.Tasks;

namespace System.Threading
{
    public class SkyApmTimer : MarshalByRefObject, IAsyncDisposable, IDisposable
    {
        private readonly Timer _timer;

        public SkyApmTimer(TimerCallback callback)
        {
            _timer = new Timer(callback.WithSkyApm());
        }

        public SkyApmTimer(TimerCallback callback, object state, int dueTime, int period)
        {
            _timer = new Timer(callback.WithSkyApm(), state, dueTime, period);
        }
        
        public SkyApmTimer(TimerCallback callback, object state, long dueTime, long period)
        {
            _timer = new Timer(callback.WithSkyApm(), state, dueTime, period);
        }

        public SkyApmTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            _timer = new Timer(callback.WithSkyApm(), state, dueTime, period);
        }

        [CLSCompliant(false)]
        public SkyApmTimer(TimerCallback callback, object state, uint dueTime, uint period)
        {
            _timer = new Timer(callback.WithSkyApm(), state, dueTime, period);
        }

        public bool Change(int dueTime, int period)
        {
            return _timer.Change(dueTime, period);
        }

        public bool Change(long dueTime, long period)
        {
            return _timer.Change(dueTime, period);
        }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            return _timer.Change(dueTime, period);
        }

        [CLSCompliant(false)]
        public bool Change(uint dueTime, uint period)
        {
            return _timer.Change(dueTime, period);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public bool Dispose(WaitHandle notifyObject)
        {
            return _timer.Dispose(notifyObject);
        }

        public ValueTask DisposeAsync()
        {
            return _timer.DisposeAsync();
        }
    }
}
