using FluentAssertions;
using HelloQuantum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Xunit;

namespace HelloQuantumTests
{
    public class OrderFindingTests
    {
        [Fact]
        public void ModMultSimpleTests()
        {
            long n = 3;
            long x = 2;
            long y = 2;
            long expected = 1;

            var transform = NumberTheoryTransforms.ModMult(x, n);

            var input = y.ToBitsPad(2).Select(bit => bit ? Qubit.ClassicOne : Qubit.ClassicZero).ToArray();
            var res = transform.Transform(new MultiQubit(input)).ToArray();

            // only one basis should have anything
            res.Where(r => r.Magnitude > 0).Single();

            long finalAnswer = -1;
            foreach (long i in LongExt.Range(0, res.LongLength))
            {
                if (res[i] == 1)
                {
                    finalAnswer = i;
                }
            }
            finalAnswer.Should().Be(expected);
        }

        [Fact]
        public void ModMultTests()
        {
            long n = 21;
            long x = 5;
            long y = 6;
            long expected = 9;

            var transform = NumberTheoryTransforms.ModMult(x, n);

            var input = y.ToBitsPad(5).Select(bit => bit ? Qubit.ClassicOne : Qubit.ClassicZero).ToArray();
            var res = transform.Transform(new MultiQubit(input)).ToArray();

            // only one basis should have anything
            res.Where(r => r.Magnitude > 0).Single();

            long finalAnswer = -1;
            foreach(long i in LongExt.Range(0, res.LongLength))
            {
                if (res[i] == 1)
                {
                    finalAnswer = i;
                }
            }
            finalAnswer.Should().Be(expected);
        }

        [Fact]
        public void ModMultExpTests()
        {
            long n = 21;
            long x = 5;
            long y = 6;
            long expected = 3;

            var transform = NumberTheoryTransforms.ModMult(x, n).Pow(2);

            var input = y.ToBitsPad(5).Select(bit => bit ? Qubit.ClassicOne : Qubit.ClassicZero).ToArray();
            var res = transform.Transform(new MultiQubit(input)).ToArray();

            // only one basis should have anything
            res.Where(r => r.Magnitude > 0).Single();

            long finalAnswer = -1;
            foreach (long i in LongExt.Range(0, res.LongLength))
            {
                if (res[i] == 1)
                {
                    finalAnswer = i;
                }
            }
            finalAnswer.Should().Be(expected);
        }

        [Fact]
        public void OrderFindSuperSmallDebug()
        {
            long x = 2;
            long n = 3;

            int l = n.BitsCeiling();
            int t = l;// OrderFindingTransform.GetPercision(n);

            var regs = OrderFindingTransform.Registers(l, t).ToArray();

            IUnitaryTransform orderfinder = OrderFindingTransform.GetStart(x, n, t);

            var regOne = MultiQubit.BasisVector(1, l);
            var regTwo = new MultiQubit(Enumerable.Range(0, t).Select(i => Qubit.ClassicZero).ToArray());
            var input = new MultiQubit(regOne, regTwo);

            IQuantumState res = orderfinder.Transform(input);

            string inputStr = input.Print(regs);
            inputStr.Should().Be("+1.00|1>|0>");
            string outStr = res.Print(regs);
            outStr.Should().Be("+0.50|1>|0>+0.50|2>|1>+0.50|1>|2>+0.50|2>|3>");
        }
    }
}
