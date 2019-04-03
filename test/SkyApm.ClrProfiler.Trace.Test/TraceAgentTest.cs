using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SkyApm.ClrProfiler.Trace.Test
{
    public class TraceAgentTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TraceAgentTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TraceAgentInitTest()
        {
            TraceAgent.GetInstance();
        }

        [Fact]
        public async Task DataReadWrapperTest()
        {
            var metaToken = this.GetType()
                .GetMethod("DataRead", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetMetadataToken();

            await DataRead("", metaToken);

            await DataReadWrapper("", metaToken);
        }

        [Fact]
        public void DataReadBenchmark()
        {
            var metaToken = this.GetType()
                .GetMethod("DataRead", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetMetadataToken();

            int m = 0;
            var flag = true;
            do
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                const int c = 10000;
                CountdownEvent k = new CountdownEvent(c);
                var flag0 = flag;
                Parallel.For(0, c, (i) =>
                {
                    var task = flag0 ? DataRead("", metaToken) : DataReadWrapper("", metaToken);
                    task.ContinueWith(n =>
                    {
                        if (n.IsFaulted)
                        {
                            _testOutputHelper.WriteLine($"{i} {n.Exception}");
                        }

                        k.Signal(1);
                    });
                });
                k.Wait();
                var name = flag0 ? "DataRead" : "DataReadWrapper";
                _testOutputHelper.WriteLine($"{name} ElapsedMilliseconds: " + sw.ElapsedMilliseconds);
                flag = !flag;
            } while (m++ <= 10);
        }

        private Task DataRead(string a, int b)
        {
            return Task.Delay(10);
        }

        private Task DataReadWrapper(string a, int b)
        {
            object ret = null;
            Exception ex = null;
            MethodTrace methodTrace = null;
            try
            {
                methodTrace = (MethodTrace) ((TraceAgent) TraceAgent.GetInstance())
                    .BeforeMethod(this.GetType(), this, new object[] {a, b}, (uint)b);

                ret = Task.Delay(10);
                goto T;
            }
            catch (Exception e)
            {
                ex = e;
                throw;
            }
            finally
            {
                if (methodTrace != null)
                {
                    methodTrace.AfterMethod(ret, ex);
                }
            }
            T:
            return (Task)ret;
        }
    }
}
