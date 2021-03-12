using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using SkyApm.Common;
using System.Text.RegularExpressions;

namespace SkyApm.Core.Tests
{
    public class ExceptionExtensionTests
    {
        [Fact]
        public void InnerException_Should_Generate_Correct()
        {
            var exception = new Exception("first level exception", new Exception("second level exception", new Exception("third level exception")));
            var result = exception.ToDemystifiedString();
            Assert.Matches(@"System.Exception: first level exception\s+---> System\.Exception: second level exception\s+---> System\.Exception: third level exception\s+--- End of inner exception stack trace ---\s+--- End of inner exception stack trace ---", result);
        }

        [Fact]
        public void InnerException_With_StackTrace_Should_Generate_Correct()
        {
            try
            {
                FirstLevelException();
                Assert.False(true, "Should never got here");
            }
            catch (Exception ex)
            {
                var result = ex.ToDemystifiedString();
                Assert.Matches(@"System\.Exception: first level exception\s+at SkyApm\.Core\.Tests\.ExceptionExtensionTests\.FirstLevelException\(\).*\s+at SkyApm\.Core\.Tests\.ExceptionExtensionTests\.InnerException_With_StackTrace_Should_Generate_Correct\(\).*\s+---> System\.Exception: second level exception\s+at SkyApm\.Core\.Tests\.ExceptionExtensionTests\.SecondLevelException\(\).*\s+at SkyApm\.Core\.Tests\.ExceptionExtensionTests\.FirstLevelException\(\).*\s+---> System\.NotImplementedException: not implemented\s+at SkyApm\.Core\.Tests\.ExceptionExtensionTests\.SecondLevelException\(\).*\s+--- End of inner exception stack trace ---\s+--- End of inner exception stack trace ---", result);
            }
        }

        [Fact]
        public void AggregateException_Should_Generate_Correct()
        {
            var exception = new AggregateException(new Exception("first exception"), new Exception("second exception"));
            var result = exception.ToDemystifiedString(0);

            Assert.Matches(@"System\.AggregateException: One or more errors occurred\. \(first exception\) \(second exception\)\s+---> System\.Exception: first exception\s+--- End of inner exception stack trace ---\s+---> System\.Exception: second exception\s+--- End of inner exception stack trace ---", result);
        }

        [Fact]
        public void InnerExceptions_Exceed_Max_Depth_Should_Ignored()
        {
            var exception = new Exception("first level exception", new Exception("second level exception", new Exception("third level exception")));
            var result = exception.ToDemystifiedString(2);
            Assert.Matches(@"System\.Exception: first level exception\s+---> System\.Exception: second level exception\s+--- End of inner exception stack trace ---", result);
        }

        [Fact]
        public void AggregateException_InnerExceptions_Exceed_Max_Depth_Should_Ignored()
        {
            var exception = new AggregateException(new Exception("first exception"), new Exception("second exception"), new Exception("third exception"));
            var result = exception.ToDemystifiedString(2);
            Assert.Matches(@"System\.AggregateException: One or more errors occurred\. \(first exception\) \(second exception\) \(third exception\)\s+---> System\.Exception: first exception\s+--- End of inner exception stack trace ---", result);
        }

        private void FirstLevelException()
        {
            try
            {
                SecondLevelException();
            }
            catch (Exception ex)
            {
                throw new Exception("first level exception", ex);
            }
        }

        private void SecondLevelException()
        {
            try
            {
                throw new NotImplementedException("not implemented");
            }
            catch (Exception ex)
            {
                throw new Exception("second level exception", ex);
            }
        }

    }
}
