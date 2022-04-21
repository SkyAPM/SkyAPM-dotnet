using FreeSql.Aop;
using SkyApm.Config;
using static SkyApm.Diagnostics.FreeSql.BaseFreeSqlTracingDiagnosticProcessor;

namespace SkyApm.Diagnostics.FreeSql
{
    public class FreeSqlTracingDiagnosticProcessorAdapter : IFreeSqlTracingDiagnosticProcessor
    {
        private readonly IFreeSqlTracingDiagnosticProcessor _processor;

        public FreeSqlTracingDiagnosticProcessorAdapter(
            FreeSqlTracingDiagnosticProcessor defaultProcessor,
            SpanFreeSqlTracingDiagnosticProcessor spanProcessor,
            IConfigAccessor configAccessor)
        {
            var instrumentConfig = configAccessor.Get<InstrumentConfig>();
            _processor = instrumentConfig.IsSpanStructure() ? (IFreeSqlTracingDiagnosticProcessor)spanProcessor : defaultProcessor;
        }

        public string ListenerName => "FreeSqlDiagnosticListener";

        [DiagnosticName(FreeSql_CurdBefore)]
        public void CurdBefore([Object] CurdBeforeEventArgs eventData)
        {
            _processor.CurdBefore(eventData);
        }

        [DiagnosticName(FreeSql_CurdAfter)]
        public void CurdAfter([Object] CurdAfterEventArgs eventData)
        {
            _processor.CurdAfter(eventData);
        }

        [DiagnosticName(FreeSql_SyncStructureBefore)]
        public void SyncStructureBefore([Object] SyncStructureBeforeEventArgs eventData)
        {
            _processor.SyncStructureBefore(eventData);
        }

        [DiagnosticName(FreeSql_SyncStructureAfter)]
        public void SyncStructureAfter([Object] SyncStructureAfterEventArgs eventData)
        {
            _processor.SyncStructureAfter(eventData);
        }

        [DiagnosticName(FreeSql_CommandBefore)]
        public void CommandBefore([Object] CommandBeforeEventArgs eventData)
        {
            _processor.CommandBefore(eventData);
        }

        [DiagnosticName(FreeSql_CommandAfter)]
        public void CommandAfter([Object] CommandAfterEventArgs eventData)
        {
            _processor.CommandAfter(eventData);
        }

        [DiagnosticName(FreeSql_TraceBefore)]
        public void TraceBeforeUnitOfWork([Object] TraceBeforeEventArgs eventData)
        {
            _processor.TraceBeforeUnitOfWork(eventData);
        }

        [DiagnosticName(FreeSql_TraceAfter)]
        public void TraceAfterUnitOfWork([Object] TraceAfterEventArgs eventData)
        {
            _processor.TraceAfterUnitOfWork(eventData);
        }
    }
}
