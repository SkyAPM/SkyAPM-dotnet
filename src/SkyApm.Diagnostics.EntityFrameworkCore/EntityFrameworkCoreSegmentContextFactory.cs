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

using System.Collections.Generic;
using System.Data.Common;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Diagnostics.EntityFrameworkCore
{
    public class EntityFrameworkCoreSegmentContextFactory : IEntityFrameworkCoreSegmentContextFactory
    {
        private readonly IEnumerable<IEntityFrameworkCoreSpanMetadataProvider> _spanMetadataProviders;
        private readonly ITracingContext _tracingContext;

        public EntityFrameworkCoreSegmentContextFactory(
            IEnumerable<IEntityFrameworkCoreSpanMetadataProvider> spanMetadataProviders,
            ITracingContext tracingContext)
        {
            _spanMetadataProviders = spanMetadataProviders;
            _tracingContext = tracingContext;
        }

        public SpanOrSegmentContext GetCurrentContext(DbCommand dbCommand)
        {
            foreach (var provider in _spanMetadataProviders)
                if (provider.Match(dbCommand.Connection))
                    return _tracingContext.CurrentExit;

            return _tracingContext.CurrentLocal;
        }

        public SpanOrSegmentContext Create(string operationName, DbCommand dbCommand)
        {
            foreach (var provider in _spanMetadataProviders)
                if (provider.Match(dbCommand.Connection))
                    return CreateExitSegment(operationName, dbCommand, provider);

            return CreateLocalSegment(operationName, dbCommand);
        }

        public void Release(SpanOrSegmentContext spanOrSegment)
        {
            _tracingContext.Finish(spanOrSegment);
        }

        private SpanOrSegmentContext CreateExitSegment(string operationName, DbCommand dbCommand,
            IEntityFrameworkCoreSpanMetadataProvider metadataProvider)
        {
            var spanOrSegment = _tracingContext.CreateExit(operationName,
                metadataProvider.GetPeer(dbCommand.Connection));
            spanOrSegment.Span.Component = new StringOrIntValue(metadataProvider.Component);
            return spanOrSegment;
        }

        private SpanOrSegmentContext CreateLocalSegment(string operationName, DbCommand dbCommand)
        {
            var spanOrSegment = _tracingContext.CreateLocal(operationName);
            spanOrSegment.Span.Component = Common.Components.ENTITYFRAMEWORKCORE;
            return spanOrSegment;
        }
    }
}