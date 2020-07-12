using FluentAssertions;
using HelloQuantum;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace HelloQuantumTests
{
    public class GroverTests
    {
        [Fact]
        public void GroverTest()
        {
            var func = new[]
            {
                false, 
                false, 
                true, 
                false
            };
            long expected = 2;
            Grover.Find(func).Should().Be(expected);
        }

        [Fact]
        public void GroverBigTest()
        {
            var func = new[]
            {
                false,
                false,
                false,
                false,
                false,
                false,
                true,
                false
            };
            long expected = 6;
            Grover.Find(func).Should().Be(expected);
        }
    }
}
