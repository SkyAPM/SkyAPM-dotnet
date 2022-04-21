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

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SkyApm.Config;

namespace SkyApm.Diagnostics.EntityFrameworkCore
{
    public class EntityFrameworkCoreTracingDiagnosticProcessor : BaseEntityFrameworkCoreTracingDiagnosticProcessor, IEntityFrameworkCoreTracingDiagnosticProcessor
    {
        private Func<CommandEventData, string> _operationNameResolver;
        private readonly IEntityFrameworkCoreSegmentContextFactory _contextFactory;
        private readonly TracingConfig _tracingConfig;
        private readonly bool _logParameterValue;

        public string ListenerName => DbLoggerCategory.Name;

        /// <summary>
        /// A delegate that returns the OpenTracing "operation name" for the given command.
        /// </summary>
        public Func<CommandEventData, string> OperationNameResolver
        {
            get
            {
                return _operationNameResolver ??
                       (_operationNameResolver = (data) =>
                       {
                           var commandType = data.Command.CommandText?.Split(' ');
                           return "DB " + (commandType.FirstOrDefault() ?? data.ExecuteMethod.ToString());
                       });
            }
            set => _operationNameResolver = value ??
                                            throw new ArgumentNullException(nameof(OperationNameResolver));
        }

        public EntityFrameworkCoreTracingDiagnosticProcessor(
            IEntityFrameworkCoreSegmentContextFactory contextFactory, IConfigAccessor configAccessor)
        {
            _contextFactory = contextFactory;
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _logParameterValue = configAccessor.Get<SamplingConfig>().LogSqlParameterValue;
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
        public void CommandExecuting([Object] CommandEventData eventData)
        {
            var operationName = OperationNameResolver(eventData);
            var context = _contextFactory.Create(operationName, eventData.Command);

            CommandExecutingSetupSpan(context.Span, eventData, _logParameterValue);
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")]
        public void CommandExecuted([Object] CommandExecutedEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            var context = _contextFactory.GetCurrentContext(eventData.Command);
            if (context != null)
            {
                _contextFactory.Release(context);
            }
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandError")]
        public void CommandError([Object] CommandErrorEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            var context = _contextFactory.GetCurrentContext(eventData.Command);
            if (context != null)
            {
                CommandErrorSetupSpan(_tracingConfig, context.Span, eventData);
                _contextFactory.Release(context);
            }
        }
    }
}