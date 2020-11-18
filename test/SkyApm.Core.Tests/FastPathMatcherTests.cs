using SkyApm.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SkyApm.Core.Tests
{
    public class FastPathMatcherTests
    {
        [Fact]
        public void Match_WithCorrectPattern_ShouldSuccess()
        {
            var path = "http://localhost:5001/api/values";

            var pattern = "http://localhost:5001/api/values";
            var result = FastPathMatcher.Match(pattern, path);
            Assert.True(result);

            pattern = "*//localhost:5001/api/values";
            result = FastPathMatcher.Match(pattern, path);
            Assert.True(result);

            pattern = "**/localhost:5001/api/values";
            result = FastPathMatcher.Match(pattern, path);
            Assert.True(result);

            pattern = "**/localhost:5001/**";
            result = FastPathMatcher.Match(pattern, path);
            Assert.True(result);

            pattern = "**localhost:5001**";
            result = FastPathMatcher.Match(pattern, path);
            Assert.True(result);
        }

        [Fact]
        public void Match_WithWrongPattern_ShouldFail()
        {
            var path = "http://localhost:5001/api/values";

            var pattern = "localhost:5001/api/values";
            var result = FastPathMatcher.Match(pattern, path);
            Assert.False(result);

            pattern = "//localhost:5001/api/values";
            result = FastPathMatcher.Match(pattern, path);
            Assert.False(result);

            pattern = "*localhost:5001/api/values";
            result = FastPathMatcher.Match(pattern, path);
            Assert.False(result);

            pattern = "**/LOCALHOST:5001/**";
            result = FastPathMatcher.Match(pattern, path);
            Assert.False(result);
        }
    }
}
