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

namespace System.Threading
{
    public static class SkyApmThreadPool
    {
        public static bool QueueUserWorkItem(WaitCallback callBack)
        {
            return ThreadPool.QueueUserWorkItem(callBack.WithSkyApm());
        }

        public static bool QueueUserWorkItem(WaitCallback callBack, object state)
        {
            return ThreadPool.QueueUserWorkItem(callBack.WithSkyApm(), state);
        }

        public static bool QueueUserWorkItem<TState>(Action<TState> callBack, TState state, bool preferLocal)
        {
            return ThreadPool.QueueUserWorkItem(callBack.WithSkyApm(), state, preferLocal);
        }

        public static bool UnsafeQueueUserWorkItem(IThreadPoolWorkItem callBack, bool preferLocal)
        {
            return ThreadPool.UnsafeQueueUserWorkItem(new SkyApmThreadPoolWorkItem(callBack), preferLocal);
        }

        public static bool UnsafeQueueUserWorkItem(WaitCallback callBack, object state)
        {
            return ThreadPool.UnsafeQueueUserWorkItem(callBack.WithSkyApm(), state);
        }

        public static bool UnsafeQueueUserWorkItem<TState>(Action<TState> callBack, TState state, bool preferLocal)
        {
            return ThreadPool.UnsafeQueueUserWorkItem(callBack.WithSkyApm(), state, preferLocal);
        }
    }
}
