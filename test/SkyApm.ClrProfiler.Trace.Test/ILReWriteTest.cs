using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace SkyApm.ClrProfiler.Trace.Test
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// ILReWriteTest
    /// </summary>
    public class ILReWriteTest
    {
        public ILReWriteTest()
        {
            var env = Environment.GetEnvironmentVariable("CORECLR_PROFILER");
            if (string.IsNullOrEmpty(env))
            {
                throw new ArgumentException("CORECLR_PROFILER Env Empty");
            }
        }

        private Task Test1(string a, int b)
        {
            return Task.Delay(10);
        }

        /// <summary>
        /// check method params name and all
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ParamsCheckTest()
        {
            var methodInfo = this.GetType()
                .GetMethod("Test1", BindingFlags.Instance | BindingFlags.NonPublic);

            var beforeMethodBody = methodInfo.GetMethodBody();

            await Test1("", 0);

            var methodBody = methodInfo.GetMethodBody();

            Assert.Equal(methodBody.LocalVariables.Count, beforeMethodBody.LocalVariables.Count + 3);
        }

        private static void StaticNoReturn(string a, ref int b)
        {

        }

        /// <summary>
        /// Static NoReturn Method Test
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void StaticNoReturnTest()
        {
            var methodInfo = this.GetType()
                .GetMethod("StaticNoReturn", BindingFlags.Static | BindingFlags.NonPublic);

            var beforeMethodBody = methodInfo.GetMethodBody();

            int i = 1;
            StaticNoReturn("", ref i);

            var methodBody = methodInfo.GetMethodBody();

            Assert.Equal(methodBody.LocalVariables.Count, beforeMethodBody.LocalVariables.Count + 3);
        }
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// just for multi ILReWriteTest, check config classes info
    /// </summary>
    public class ILReWriteTest2
    {
        public ILReWriteTest2()
        {
            var env = Environment.GetEnvironmentVariable("CORECLR_PROFILER");
            if (string.IsNullOrEmpty(env))
            {
                throw new ArgumentException("CORECLR_PROFILER Env Empty");
            }
        }

        private Task Test2(string a, int b)
        {
            return Task.Delay(10);
        }

        /// <summary>
        /// MethodNameCheckTest no check method params name , just check name
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MethodNameCheckTest()
        {
            var methodInfo = this.GetType()
                .GetMethod("Test2", BindingFlags.Instance | BindingFlags.NonPublic);

            var beforeMethodBody = methodInfo.GetMethodBody();

            await Test2("", 0);

            var methodBody = methodInfo.GetMethodBody();

            Assert.Equal(methodBody.LocalVariables.Count, beforeMethodBody.LocalVariables.Count + 3);
        }
    }
}
