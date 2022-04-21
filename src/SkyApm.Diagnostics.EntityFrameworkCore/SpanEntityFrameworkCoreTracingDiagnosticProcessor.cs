using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace SkyApm.Diagnostics.EntityFrameworkCore
{
    public class SpanEntityFrameworkCoreTracingDiagnosticProcessor : BaseEntityFrameworkCoreTracingDiagnosticProcessor, IEntityFrameworkCoreTracingDiagnosticProcessor
    {
        private readonly IEnumerable<IEntityFrameworkCoreSpanMetadataProvider> _spanMetadataProviders;
        private Func<CommandEventData, string> _operationNameResolver;
        private readonly ITracingContext _tracingContext;
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

        public SpanEntityFrameworkCoreTracingDiagnosticProcessor(
            IEnumerable<IEntityFrameworkCoreSpanMetadataProvider> spanMetadataProviders,
            ITracingContext tracingContext,
            IConfigAccessor configAccessor)
        {
            _spanMetadataProviders = spanMetadataProviders;
            _tracingContext = tracingContext;
            _tracingConfig = configAccessor.Get<TracingConfig>();
            _logParameterValue = configAccessor.Get<SamplingConfig>().LogSqlParameterValue;
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
        public void CommandExecuting([Object] CommandEventData eventData)
        {
            var operationName = OperationNameResolver(eventData);
            SegmentSpan span = null;
            foreach (var provider in _spanMetadataProviders)
            {
                if (provider.Match(eventData.Command.Connection))
                {
                    span = _tracingContext.CreateExitSpan(operationName, provider.GetPeer(eventData.Command.Connection));
                    span.Component = new StringOrIntValue(provider.Component);
                    break;
                }
            }

            if (span == null)
            {
                span = _tracingContext.CreateLocalSpan(operationName);
                span.Component = Components.ENTITYFRAMEWORKCORE;
            }

            CommandExecutingSetupSpan(span, eventData, _logParameterValue);
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")]
        public void CommandExecuted([Object] CommandExecutedEventData eventData)
        {
            if (eventData == null) return;

            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            _tracingContext.StopSpan(span);
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandError")]
        public void CommandError([Object] CommandErrorEventData eventData)
        {
            if (eventData == null) return;

            var span = _tracingContext.ActiveSpan;
            if (span == null) return;

            CommandErrorSetupSpan(_tracingConfig, span, eventData);
            _tracingContext.StopSpan(span);
        }
    }
}
