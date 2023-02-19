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
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics.MassTransit.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SkyApm.Diagnostics.MassTransit.Observers
{
    public class MasstransitReceiveObserver : IReceiveObserver
    {
        private readonly ConcurrentDictionary<Guid, SegmentContext> _contexts = new ConcurrentDictionary<Guid, SegmentContext>();

        private readonly ITracingContext _tracingContext;
        private IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private TracingConfig _tracingConfig;
        public MasstransitReceiveObserver(ITracingContext tracingContext,
            IEntrySegmentContextAccessor entrySegmentContextAccessor,
            IConfigAccessor configAccessor)
        {
            _tracingContext = tracingContext;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            _tracingConfig = configAccessor.Get<TracingConfig>();
        }

        public Task PreReceive(ReceiveContext context)
        {
            //activity will be null in aspnet.core web
            //we need to config the ActivityListener
            ///<seealso cref="MasstransitSkyApmHostingStartup"/>
            var activity = Activity.Current ?? default;

            var segContext = _tracingContext.CreateEntrySegmentContext("Masstransit Receiving/ " + activity.OperationName,
                new MasstransitCarrierHeaderCollection(context.TransportHeaders));
            //Mostly the receive point is from MQ
            segContext.Span.SpanLayer = SpanLayer.MQ;
            segContext.Span.Component = Components.ASPNETCORE;
            segContext.Span.Peer = context.InputAddress?.Host;
            segContext.Span.AddTag(Tags.DB_TYPE, "Sql");
            segContext.Span.AddTag(MassTags.InputAddress, context.InputAddress?.Host);
            segContext.Span.AddLog(LogEvent.Event("Masstransit Message Receiving Start"));
            segContext.Span.AddLog(LogEvent.Message("Masstransit message received start..."));

            _contexts[context.GetMessageId().Value] = _entrySegmentContextAccessor.Context;
            return Task.CompletedTask;
        }

        public Task PostReceive(ReceiveContext context)
        {
            var segContext = _entrySegmentContextAccessor.Context;
            if (segContext == null) return Task.CompletedTask;

            var activity = Activity.Current ?? default;

            foreach (var tags in activity.Tags)
            {
                segContext.Span.AddTag(tags.Key, tags.Value);
            }
            segContext.Span.AddLog(LogEvent.Event("Masstransit Message Received End"));
            segContext.Span.AddLog(LogEvent.Message($"Masstransit message Received succeeded!{Environment.NewLine}" +
                                                 $"--> Spend Time: { activity.Duration.TotalMilliseconds }ms.{Environment.NewLine}" +
                                                 $"--> Message Id: { context.GetMessageId() } , Name: { activity.OperationName}"));

            _tracingContext.Release(segContext);
            _contexts.TryRemove(context.GetMessageId().Value, out _);
            return Task.CompletedTask;
        }

        public Task ReceiveFault(ReceiveContext context, Exception exception)
        {
            var segContext = _entrySegmentContextAccessor.Context;
            if (segContext == null) return Task.CompletedTask;

            var activity = Activity.Current ?? default;

            foreach (var tags in activity.Tags)
            {
                segContext.Span.AddTag(tags.Key, tags.Value);
            }
            segContext.Span.AddLog(LogEvent.Event("Masstransit Message Received Error"));
            segContext.Span.AddLog(LogEvent.Message($"Masstransit message received failed!{Environment.NewLine}" +
                                                 $"--> Spend Time: { activity.Duration.TotalMilliseconds }ms.{Environment.NewLine}" +
                                                 $"--> Message Id: { context.GetMessageId() } , Name: { activity.OperationName}"));
            segContext.Span.ErrorOccurred(exception, _tracingConfig);
            _tracingContext.Release(segContext);
            _contexts.TryRemove(context.GetMessageId().Value, out _);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 在PostReceive之前进行，在IConsumeObserver的PostConsume之后进行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="duration"></param>
        /// <param name="consumerType"></param>
        /// <returns></returns>
        public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) where T : class
        {
            return Task.CompletedTask;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception) where T : class
        {
            return Task.CompletedTask;
        }
    }
}
