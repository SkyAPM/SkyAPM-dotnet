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

namespace System.Threading.Tasks
{
    public class SkyApmTask : Task
    {
        public SkyApmTask(Action action) : base(action.WithSkyApm())
        {
        }

        public SkyApmTask(Action action, CancellationToken cancellationToken) : base(action.WithSkyApm(), cancellationToken)
        {
        }

        public SkyApmTask(Action action, TaskCreationOptions creationOptions) : base(action.WithSkyApm(), creationOptions)
        {
        }

        public SkyApmTask(Action<object> action, object state) : base(action.WithSkyApm(), state)
        {
        }

        public SkyApmTask(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action.WithSkyApm(), cancellationToken, creationOptions)
        {
        }

        public SkyApmTask(Action<object> action, object state, CancellationToken cancellationToken) : base(action.WithSkyApm(), state, cancellationToken)
        {
        }

        public SkyApmTask(Action<object> action, object state, TaskCreationOptions creationOptions) : base(action.WithSkyApm(), state, creationOptions)
        {
        }

        public SkyApmTask(Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action.WithSkyApm(), state, cancellationToken, creationOptions)
        {
        }

        public static new Task Run(Action action)
        {
            return Task.Run(action.WithSkyApm());
        }

        public static new Task Run(Action action, CancellationToken cancellationToken)
        {
            return Task.Run(action.WithSkyApm(), cancellationToken);
        }

        public static new Task Run(Func<Task> function)
        {
            return Task.Run(function.WithSkyApm());
        }

        public static new Task Run(Func<Task> function, CancellationToken cancellationToken)
        {
            return Task.Run(function.WithSkyApm(), cancellationToken);
        }

        public static new Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return Task.Run(function.WithSkyApm());
        }

        public static new Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            return Task.Run(function.WithSkyApm(), cancellationToken);
        }

        public static new Task<TResult> Run<TResult>(Func<TResult> function)
        {
            return Task.Run(function.WithSkyApm());
        }

        public static new Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken)
        {
            return Task.Run(function.WithSkyApm(), cancellationToken);
        }
    }
}
