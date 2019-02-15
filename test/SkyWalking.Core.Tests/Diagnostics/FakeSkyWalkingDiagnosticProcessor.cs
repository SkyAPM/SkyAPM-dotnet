using System;
using SkyApm.Diagnostics;
using Xunit;
    
namespace SkyApm.Core.Tests.Diagnostics
{
    public class FakeTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = FakeDiagnosticListener.ListenerName;
        
        public DateTime Timestamp { get; set; }

        [DiagnosticName(FakeDiagnosticListener.Executing)]
        public void Executing(
            [Property(Name = "Name")] string eventName, 
            [Property] DateTime Timestamp)
        {
            Assert.Equal("Executing", eventName);
            this.Timestamp = Timestamp;
        }

        [DiagnosticName(FakeDiagnosticListener.Executed)]
        public void Executed([Object] FakeDiagnosticListenerData data)
        {
            Assert.Equal("Executed", data.Name);
            Timestamp = data.Timestamp;
        }
    }
}