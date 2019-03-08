using System;
using System.Collections.Generic;
using System.Text;
using SmartSql;
using SmartSql.Diagnostics;

namespace SkyApm.Diagnostics.SmartSql
{
    public class SmartSqlTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName => SmartSqlDiagnosticListenerExtensions.SMART_SQL_DIAGNOSTIC_LISTENER;


    }
}
