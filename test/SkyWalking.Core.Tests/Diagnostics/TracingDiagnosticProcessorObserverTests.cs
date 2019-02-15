using System;
using System.Diagnostics;
using SkyApm.Diagnostics;
using SkyApm.Logging;
using Xunit;

namespace SkyApm.Core.Tests.Diagnostics
{
    public class TracingDiagnosticProcessorObserverTests
    {
        [Fact]
        public void Property_Binder_Invoke_Test()
        {
            var listener = new FakeDiagnosticListener();
            var fakeProcessor = new FakeTracingDiagnosticProcessor();
            var observer = new TracingDiagnosticProcessorObserver(new ITracingDiagnosticProcessor[] {fakeProcessor},new NullLoggerFactory());
            DiagnosticListener.AllListeners.Subscribe(observer);

            var timeStamp = DateTime.Now;
            listener.Write(FakeDiagnosticListener.Executing,
                new
                {
                    Name = FakeDiagnosticListener.Executing,
                    Timestamp = timeStamp
                });

            Assert.Equal(timeStamp, fakeProcessor.Timestamp);
        }

        [Fact]
        public void Object_Binder_Invoke_Test()
        {
            var listener = new FakeDiagnosticListener();
            var fakeProcessor = new FakeTracingDiagnosticProcessor();
            var observer = new TracingDiagnosticProcessorObserver(new ITracingDiagnosticProcessor[] {fakeProcessor},
                new NullLoggerFactory());
            DiagnosticListener.AllListeners.Subscribe(observer);

            var timeStamp = DateTime.Now;

            listener.Write(FakeDiagnosticListener.Executed,
                new FakeDiagnosticListenerData
                {
                    Name = FakeDiagnosticListener.Executed,
                    Timestamp = timeStamp
                });

            Assert.Equal(timeStamp, fakeProcessor.Timestamp);
        }
    }
}