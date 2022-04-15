using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Transport;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SkyApm.Core.Logging
{
    public class SkyApmLoggerProvider : IDisposable, ILoggerProvider, ISupportExternalScope
    {
        internal FileLoggerOptions Settings { get; private set; }
        bool Terminated;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        ConcurrentQueue<LoggerContext> logQueues = new ConcurrentQueue<LoggerContext>();
        ConcurrentDictionary<string, SkyApmLogger> loggers = new ConcurrentDictionary<string, SkyApmLogger>();
        IExternalScopeProvider fScopeProvider;
        protected IDisposable SettingsChangeToken;
        private readonly ISkyApmLogDispatcher _skyApmLogDispatcher;
        /// <summary>
        /// 
        /// </summary>
        public SkyApmLoggerProvider(FileLoggerOptions Settings, IServiceProvider serviceProvider)
        {
            _entrySegmentContextAccessor = serviceProvider.GetService<IEntrySegmentContextAccessor>();
            _skyApmLogDispatcher = serviceProvider.GetService<ISkyApmLogDispatcher>();
        }

        public SkyApmLoggerProvider(IOptionsMonitor<FileLoggerOptions> Settings, IServiceProvider serviceProvider)
            : this(Settings.CurrentValue, serviceProvider)
        {
            SettingsChangeToken = Settings.OnChange(settings =>
            {
                this.Settings = settings;
            });
        }

        void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            fScopeProvider = scopeProvider;
        }

        ILogger ILoggerProvider.CreateLogger(string Category)
        {
            return loggers.GetOrAdd(Category,
            (category) =>
            {
                return new SkyApmLogger(this, _entrySegmentContextAccessor, _skyApmLogDispatcher, category);
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (SettingsChangeToken != null)
            {
                SettingsChangeToken.Dispose();
                SettingsChangeToken = null;
            }
        }

        internal IExternalScopeProvider ScopeProvider
        {
            get
            {
                if (fScopeProvider == null)
                    fScopeProvider = new LoggerExternalScopeProvider();
                return fScopeProvider;
            }
        }

        public bool IsDisposed { get; protected set; }

        ~SkyApmLoggerProvider()
        {
            if (!this.IsDisposed)
            {
                Dispose(false);
            }
        }
        void IDisposable.Dispose()
        {
            if (!this.IsDisposed)
            {
                try
                {
                    Dispose(true);
                }
                catch
                {
                }

                this.IsDisposed = true;
                GC.SuppressFinalize(this);  // instructs GC not bother to call the destructor               
            }
        }

    }
}
