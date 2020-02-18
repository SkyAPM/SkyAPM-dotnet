/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using SkyApm.Tracing;
using System;
using Xunit;

namespace SkyApm.Core.Tests
{
    public class UniqueIdParserTest
    {
        private static readonly IUniqueIdParser Parser = new UniqueIdParser();

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("1", false)]
        [InlineData("1.1", false)]
        [InlineData("1.1.", false)]
        [InlineData("1.1.a", false)]
        [InlineData("1.1.1.1", false)]
        [InlineData("1\\1.-1", true)]
        public void TryParse_Return(string text, bool result) =>
            Assert.Equal(result, Parser.TryParse(text, out _));

        [Theory]
        [InlineData("1.2.3", 1, 2, 3)]
        [InlineData("123.456.789", 123, 456, 789)]
        [InlineData("-1.-2.-3", -1, -2, -3)]
        [InlineData("9223372036854775807.9223372036854775807.9223372036854775807", 9223372036854775807, 9223372036854775807, 9223372036854775807)]
        [InlineData("-9223372036854775807.-9223372036854775807.-9223372036854775807", -9223372036854775807, -9223372036854775807, -9223372036854775807)]
        public void TryParse_Out(string text, long part1, long part2, long part3)
        {
            Parser.TryParse(text, out var id);

            Assert.Equal(part1, id.Part1);
            Assert.Equal(part2, id.Part2);
            Assert.Equal(part3, id.Part3);
        }
    }
}
