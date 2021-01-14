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
    public class MongoDiagnosticsProcessor : ITracingDiagnosticProcessor
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
            context.Span.SpanLayer = Tracing.Segments.SpanLayer.DB;
            context.Span.Component = Common.Components.MongoDBCLIENT;
            context.Span.AddTag("db.system", "mongodb");
            context.Span.AddTag("db.name", @event.DatabaseNamespace?.DatabaseName);
            context.Span.AddTag("db.mongodb.collection", operationName);
            context.Span.AddTag("db.operation", operationName + @event.CommandName);
            context.Span.AddTag(Common.Tags.DB_TYPE, "sql");
            context.Span.AddTag(Common.Tags.DB_INSTANCE, @event.DatabaseNamespace.DatabaseName);
            context.Span.AddTag(Common.Tags.DB_STATEMENT, @event.Command.ToString());
        }

        [DiagnosticName("MongoActivity.Stop")]
        public void AfterExecuteCommand([Object] CommandSucceededEvent @event)
        { 
            var context = _contextAccessor.Context;
            context?.Span.AddTag(Common.Tags.STATUS_CODE, "ok");

            _tracingContext.Release(context);
        }

        [DiagnosticName("MongoActivity.Failed")]
        public void FailedExecuteCommand([Object] CommandFailedEvent @event)
        {
            var context = _contextAccessor.Context;
            context?.Span.AddTag("status_description", @event.Failure.Message);
            context?.Span.AddTag("error.type", @event.Failure.GetType().FullName);
            context?.Span.AddTag("error.msg", @event.Failure.Message);
            context?.Span.AddTag("error.stack", @event.Failure.StackTrace);
            context?.Span.AddTag(Common.Tags.STATUS_CODE, "error");

            _tracingContext.Release(context);
        }
         
    }
}
