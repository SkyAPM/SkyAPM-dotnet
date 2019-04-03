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
 
using SkyApm.ClrProfiler.Trace.Utils;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Linq;

 namespace SkyApm.ClrProfiler.Trace.StackExchangeRedis
{
    public class StackExchangeRedis : AbsMethodWrapper
    {
        private const string TypeName = "StackExchange.Redis.ConnectionMultiplexer";
        private static readonly string[] AssemblyNames = new[] { "StackExchange.Redis", "StackExchange.Redis.StrongName" };

        private const string ExecuteAsyncImpl = "ExecuteAsyncImpl";
        private const string ExecuteSyncImpl = "ExecuteSyncImpl";
        private static readonly PropertyFetcher ConfigPropertyFetcher = new PropertyFetcher("Configuration");
        private static readonly PropertyFetcher CommandAndKeyPropertyFetcher = new PropertyFetcher("CommandAndKey");
        
        private readonly ITracingContext _tracingContext;

        public StackExchangeRedis(ITracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }

        public override AfterMethodDelegate BeginWrapMethod(TraceMethodInfo traceMethodInfo)
        {
            var multiplexer = traceMethodInfo.InvocationTarget;
            var message = traceMethodInfo.MethodArguments[0];

            var config = (string)ConfigPropertyFetcher.Fetch(multiplexer);
            var hostAndPort = GetHostAndPort(config);
            var rawCommand = (string)CommandAndKeyPropertyFetcher.Fetch(message);

            var operationName = $"Cache {traceMethodInfo.MethodBase.Name}";
            var context = _tracingContext.CreateExitSegmentContext(operationName, $"{hostAndPort.Item1}:{hostAndPort.Item2}");
            context.Span.Component = Common.Components.STACKEXCHANGEREDIS;
            context.Span.SpanLayer = SpanLayer.CACHE;
            context.Span.AddTag(Common.Tags.DB_TYPE, "Cache");
            context.Span.AddTag(Common.Tags.DB_STATEMENT, rawCommand);

            traceMethodInfo.TraceContext = context;


            if (traceMethodInfo.MethodBase.Name == ExecuteSyncImpl)
            {
                return delegate (object returnValue, Exception ex)
                {
                    Leave(traceMethodInfo, returnValue, ex);
                };
            }
            else
            {
                return delegate (object returnValue, Exception ex)
                {
                    DelegateHelper.AsyncMethodEnd(Leave, traceMethodInfo, ex, returnValue);

                    _tracingContext.ReleaseScope();
                };
            }
        }

        private void Leave(TraceMethodInfo traceMethodInfo, object ret, Exception ex)
        {
            var context = (SegmentContext)traceMethodInfo.TraceContext;
            if (ex != null)
            {
                context.Span.ErrorOccurred(ex);
            }
            _tracingContext.Release(context);
        }


        /// <summary>
        /// Get the host and port from the config
        /// </summary>
        /// <param name="config">The config</param>
        /// <returns>The host and port</returns>
        private static Tuple<string, string> GetHostAndPort(string config)
        {
            string host = null;
            string port = null;

            if (config != null)
            {
                // config can contain several settings separated by commas:
                // hostname:port,name=MyName,keepAlive=180,syncTimeout=10000,abortConnect=False
                // split in commas, find the one without '=', split that one on ':'
                string[] hostAndPort = config.Split(',')
                    .FirstOrDefault(p => !p.Contains("="))
                    ?.Split(':');

                if (hostAndPort != null)
                {
                    host = hostAndPort[0];
                }

                // check length because port is optional
                if (hostAndPort?.Length > 1)
                {
                    port = hostAndPort[1];
                }
            }

            return new Tuple<string, string>(host, port);
        }

        public override bool CanWrap(TraceMethodInfo traceMethodInfo)
        {
            var invocationTargetType = traceMethodInfo.Type;
            var assemblyName = invocationTargetType.Assembly.GetName().Name;
            if (AssemblyNames.Contains(assemblyName) && TypeName == invocationTargetType.FullName)
            {
                if (traceMethodInfo.MethodBase.Name == ExecuteAsyncImpl || traceMethodInfo.MethodBase.Name == ExecuteSyncImpl)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

