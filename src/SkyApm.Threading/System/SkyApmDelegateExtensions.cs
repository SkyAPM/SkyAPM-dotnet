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
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    public static class SkyApmDelegateExtensions
    {
        #region Action
        public static Action WithSkyApm(this Action action)
        {
            var operationName = string.Concat(action.Method.DeclaringType?.FullName ?? "UNKNOW", ".", action.Method.Name);
            return WithSkyApm(action, operationName);
        }

        public static Action WithSkyApm(this Action action, string operationName)
        {
            if (action == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(operationName);
            var carrier = prepare.GetCrossThreadCarrier();

            Action apmAction = () =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + operationName, carrier);
                try
                {
                    action();
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
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }

        public static Action<object> WithSkyApm(this Action<object> action)
        {
            var operationName = string.Concat(action.Method.DeclaringType?.FullName ?? "UNKNOW", ".", action.Method.Name);
            return WithSkyApm(action, operationName);
        }

        public static Action<object> WithSkyApm(this Action<object> action, string operationName)
        {
            if (action == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(operationName);
            var carrier = prepare.GetCrossThreadCarrier();

            Action<object> apmAction = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + operationName, carrier);
                try
                {
                    action(obj);
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
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }

        public static Action<TState> WithSkyApm<TState>(this Action<TState> action)
        {
            var operationName = string.Concat(action.Method.DeclaringType?.FullName ?? "UNKNOW", ".", action.Method.Name);
            return WithSkyApm(action, operationName);
        }

        public static Action<TState> WithSkyApm<TState>(this Action<TState> action, string operationName)
        {
            if (action == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(operationName);
            var carrier = prepare.GetCrossThreadCarrier();

            Action<TState> apmAction = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + operationName, carrier);
                try
                {
                    action(obj);
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
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }
        #endregion Action

        #region Func
        public static Func<TResult> WithSkyApm<TResult>(this Func<TResult> func)
        {
            var operationName = string.Concat(func.Method.DeclaringType?.FullName ?? "UNKNOW", ".", func.Method.Name);
            return WithSkyApm(func, operationName);
        }

        public static Func<TResult> WithSkyApm<TResult>(this Func<TResult> func, string operationName)
        {
            if (func == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(operationName);
            var carrier = prepare.GetCrossThreadCarrier();

            Func<TResult> apmAction = () =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + operationName, carrier);
                try
                {
                    return func();
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
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }

        public static Func<object, TResult> WithSkyApm<TResult>(this Func<object, TResult> func)
        {
            var operationName = string.Concat(func.Method.DeclaringType?.FullName ?? "UNKNOW", ".", func.Method.Name);
            return WithSkyApm(func, operationName);
        }

        public static Func<object, TResult> WithSkyApm<TResult>(this Func<object, TResult> func, string operationName)
        {
            if (func == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(operationName);
            var carrier = prepare.GetCrossThreadCarrier();

            Func<object, TResult> apmAction = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + operationName, carrier);
                try
                {
                    return func(obj);
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
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }

        public static Func<Task> WithSkyApm(this Func<Task> func)
        {
            var operationName = string.Concat(func.Method.DeclaringType?.FullName ?? "UNKNOW", ".", func.Method.Name);
            return WithSkyApm(func, operationName);
        }

        public static Func<Task> WithSkyApm(this Func<Task> func, string operationName)
        {
            if (func == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(operationName);
            var carrier = prepare.GetCrossThreadCarrier();

            Func<Task> apmAction = async () =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + operationName, carrier);
                try
                {
                    await func();
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
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }

        public static Func<Task<TResult>> WithSkyApm<TResult>(this Func<Task<TResult>> func)
        {
            var operationName = string.Concat(func.Method.DeclaringType?.FullName ?? "UNKNOW", ".", func.Method.Name);
            return WithSkyApm(func, operationName);
        }

        public static Func<Task<TResult>> WithSkyApm<TResult>(this Func<Task<TResult>> func, string operationName)
        {
            if (func == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(operationName);
            var carrier = prepare.GetCrossThreadCarrier();

            Func<Task<TResult>> apmAction = async () =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + operationName, carrier);
                try
                {
                    return await func();
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
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmAction;
        }
        #endregion Func

        #region ThreadStart
        public static ThreadStart WithSkyApm(this ThreadStart threadStart)
        {
            var operationName = string.Concat(threadStart.Method.DeclaringType?.FullName ?? "UNKNOW", ".", threadStart.Method.Name);
            return WithSkyApm(threadStart, operationName);
        }

        public static ThreadStart WithSkyApm(this ThreadStart threadStart, string operationName)
        {
            if (threadStart == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(operationName);
            var carrier = prepare.GetCrossThreadCarrier();

            ThreadStart apmThreadStart = () =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + operationName, carrier);
                try
                {
                    threadStart();
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
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmThreadStart;
        }
        #endregion ThreadStart

        #region ParameterizedThreadStart
        public static ParameterizedThreadStart WithSkyApm(this ParameterizedThreadStart threadStart)
        {
            var operationName = string.Concat(threadStart.Method.DeclaringType?.FullName ?? "UNKNOW", ".", threadStart.Method.Name);
            return WithSkyApm(threadStart, operationName);
        }

        public static ParameterizedThreadStart WithSkyApm(this ParameterizedThreadStart threadStart, string operationName)
        {
            if (threadStart == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(operationName);
            var carrier = prepare.GetCrossThreadCarrier();

            ParameterizedThreadStart apmThreadStart = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + operationName, carrier);
                try
                {
                    threadStart(obj);
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
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmThreadStart;
        }
        #endregion ParameterizedThreadStart

        #region WaitCallback
        public static WaitCallback WithSkyApm(this WaitCallback callback)
        {
            var operationName = string.Concat(callback.Method.DeclaringType?.FullName ?? "UNKNOW", ".", callback.Method.Name);
            return WithSkyApm(callback, operationName);
        }

        public static WaitCallback WithSkyApm(this WaitCallback callback, string operationName)
        {
            if (callback == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(operationName);
            var carrier = prepare.GetCrossThreadCarrier();

            WaitCallback apmCallback = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + operationName, carrier);
                try
                {
                    callback(obj);
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
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmCallback;
        }
        #endregion WaitCallback

        #region TimerCallback
        public static TimerCallback WithSkyApm(this TimerCallback callback)
        {
            var operationName = string.Concat(callback.Method.DeclaringType?.FullName ?? "UNKNOW", ".", callback.Method.Name);
            return WithSkyApm(callback, operationName);
        }

        public static TimerCallback WithSkyApm(this TimerCallback callback, string operationName)
        {
            if (callback == null) return null;

            var prepare = SkyApmInstances.TracingContext.CreateLocal(operationName);
            var carrier = prepare.GetCrossThreadCarrier();

            TimerCallback apmCallback = obj =>
            {
                var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + operationName, carrier);
                try
                {
                    callback(obj);
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
            };

            SkyApmInstances.TracingContext.Finish(prepare);

            return apmCallback;
        }
        #endregion TimerCallback
    }
}
