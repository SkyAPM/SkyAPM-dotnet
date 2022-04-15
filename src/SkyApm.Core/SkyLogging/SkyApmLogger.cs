using Microsoft.Extensions.Logging;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Transport;
using System;
using System.Collections.Generic;

namespace SkyApm.Core.Logging
{
    public class SkyApmLogger : ILogger
    {

        public SkyApmLoggerProvider Provider { get; private set; }
        public string Category { get; private set; }
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;

        private readonly ISkyApmLogDispatcher _skyApmLogDispatcher;
        public SkyApmLogger(SkyApmLoggerProvider Provider, IEntrySegmentContextAccessor entrySegmentContextAccessor, ISkyApmLogDispatcher skyApmLogDispatcher, string Category)
        {
            this.Provider = Provider;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            this.Category = Category;
            _skyApmLogDispatcher = skyApmLogDispatcher;
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return Provider.ScopeProvider.Push(state);
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if ((this as ILogger).IsEnabled(logLevel))
            {
                var logs = new Dictionary<string, object>();
                logs.Add("className", this.Category);
                logs.Add("Level", logLevel);
                logs.Add("logMessage", exception?.Message ?? state.ToString());
                logs.Add("eventId", eventId.ToString());
                logs.Add("state", state.ToString());
                if (state is string)
                {
                    logs.Add("stateText", state.ToString());
                }
                else if (state is IEnumerable<KeyValuePair<string, object>> Properties)
                {
                    var stateProperties = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> item in Properties)
                    {
                        stateProperties.Add(item.Key, item.Value);
                    }
                    logs.Add("stateProperties", stateProperties);
                }
                var logContext = new LoggerContext()
                {
                    Logs = logs,
                    SegmentContext = _entrySegmentContextAccessor.Context,
                };
                _skyApmLogDispatcher.Dispatch(logContext);
                #region MyRegion


                //SkyApmLogEntry Info = new SkyApmLogEntry();
                //Info.Category = this.Category;
                //Info.Level = logLevel.ToString();
                //Info.Text = exception?.Message ?? state.ToString(); // formatter(state, exception)
                //Info.Exception = exception;
                //Info.EventId = eventId;
                //Info.State = state;

                //// well, you never know what it really is
                //if (state is string)
                //{
                //    Info.StateText = state.ToString();
                //}
                //// in case we have to do with a message template, lets get the keys and values (for Structured Logging providers)
                //// SEE: <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging#log-message-template">https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging#log-message-template</a>
                //// SEE: <a href="https://softwareengineering.stackexchange.com/questions/312197/benefits-of-structured-logging-vs-basic-logging">https://softwareengineering.stackexchange.com/questions/312197/benefits-of-structured-logging-vs-basic-logging</a>
                //else if (state is IEnumerable<KeyValuePair<string, object>> Properties)
                //{
                //    Info.StateProperties = new Dictionary<string, object>();

                //    foreach (KeyValuePair<string, object> item in Properties)
                //    {
                //        Info.StateProperties[item.Key] = item.Value;
                //    }
                //}

                // gather info about scope(s), if any
                //if (Provider.ScopeProvider != null)
                //{
                //    Provider.ScopeProvider.ForEachScope((value, loggingProps) =>
                //    {
                //        if (Info.Scopes == null)
                //            Info.Scopes = new List<LogScopeInfo>();

                //        LogScopeInfo Scope = new LogScopeInfo();
                //        Info.Scopes.Add(Scope);

                //        if (value is string)
                //        {
                //            Scope.Text = value.ToString();
                //        }
                //        else if (value is IEnumerable<KeyValuePair<string, object>> props)
                //        {
                //            if (Scope.Properties == null)
                //                Scope.Properties = new Dictionary<string, object>();

                //            foreach (var pair in props)
                //            {
                //                Scope.Properties[pair.Key] = pair.Value;
                //            }
                //        }
                //    },
                //    state);

                //}
                //Provider.WriteLog(Info);
                #endregion
            }
        }

    }
}
