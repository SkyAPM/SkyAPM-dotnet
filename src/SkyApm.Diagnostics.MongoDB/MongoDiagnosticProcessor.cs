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

using MongoDB.Driver.Core.Events;
using SkyApm.Tracing;
using System;

namespace SkyApm.Diagnostics.MongoDB
{
    public class MongoDiagnosticsProcessor : BaseMongoDiagnosticsProcessor, IMongoDiagnosticsProcessor
    {
        public string ListenerName => "MongoSourceListener";
        private readonly ITracingContext _tracingContext;
        private readonly IExitSegmentContextAccessor _contextAccessor;
 
        public MongoDiagnosticsProcessor(ITracingContext tracingContext,
            IExitSegmentContextAccessor contextAccessor)
        {
            _tracingContext = tracingContext;
            _contextAccessor = contextAccessor;
        }

        [DiagnosticName("MongoActivity.Start")]
        public void BeforeExecuteCommand([Object] CommandStartedEvent @event)
        {
            var operationName = DiagnosticsActivityEventSubscriber.GetCollectionName(@event);
            var context = _tracingContext.CreateExitSegmentContext(operationName, @event.ConnectionId.ServerId.EndPoint.ToString());
            BeforeExecuteCommandSetupSpan(context.Span, operationName, @event);
        }

        [DiagnosticName("MongoActivity.Stop")]
        public void AfterExecuteCommand([Object] CommandSucceededEvent @event)
        { 
            var context = _contextAccessor.Context;
            if (context == null) return;

            AfterExecuteCommandSetupSpan(context.Span, @event);

            _tracingContext.Release(context);
        }

        [DiagnosticName("MongoActivity.Failed")]
        public void FailedExecuteCommand([Object] CommandFailedEvent @event)
        {
            var context = _contextAccessor.Context;
            if (context == null) return;

            FailedExecuteCommandSetupSpan(context.Span, @event);

            _tracingContext.Release(context);
        }
         
    }
}
