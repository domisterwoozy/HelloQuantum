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
        public void PhaseHadamarTest()
        {
            long x = 2;
            long n = 3;

            int l = n.BitsCeiling();
            int t = l;// OrderFindingTransform.GetPercision(n);

            var regs = OrderFindingTransform.Registers(t, l).ToArray();

            IUnitaryTransform orderfinder = PhaseEstimator.GetPhaseHadamar(t, l);

            var regTwo = MultiQubit.BasisVector(1, l);
            var regOne = new MultiQubit(Enumerable.Range(0, t).Select(i => Qubit.ClassicZero).ToArray());
            var input = new MultiQubit(regOne, regTwo);

            IQuantumState res = orderfinder.Transform(input);

            string inputStr = input.Print(regs);
            inputStr.Should().Be("+1.00|0>|1>");
            string outStr = res.Print(regs);
            // first reg is unchanged, second reg is maximally mixed
            outStr.Should().Be("+0.50|0>|1>+0.50|1>|1>+0.50|2>|1>+0.50|3>|1>");
        }

        [Fact]
        public void OrderStartTest()
        {
            long x = 2;
            long n = 3;

            int l = n.BitsCeiling();
            int t = l;// OrderFindingTransform.GetPercision(n);

            var regs = OrderFindingTransform.Registers(t, l).ToArray();

            IUnitaryTransform orderfinder = OrderFindingTransform.GetStart(x, n, t);

            var regTwo = MultiQubit.BasisVector(1, l);
            var regOne = new MultiQubit(Enumerable.Range(0, t).Select(i => Qubit.ClassicZero).ToArray());
            var input = new MultiQubit(regOne, regTwo);

            IQuantumState res = orderfinder.Transform(input);

            string inputStr = input.Print(regs);
            inputStr.Should().Be("+1.00|0>|1>");
            string outStr = res.Print(regs);
            outStr.Should().Be("+0.50|0>|1>+0.50|1>|2>+0.50|2>|1>+0.50|3>|2>");
        }

        [Fact]
        public void OrderFindTest()
        {
            long x = 2;
            long n = 3;
            int r = 2; // 2 ^ 2 = 3 + 1

            int l = n.BitsCeiling();
            int t = OrderFindingTransform.GetPercision(n);

            var regs = OrderFindingTransform.Registers(t, l).ToArray();

            IUnitaryTransform orderfinder = OrderFindingTransform.Get(x, n, t);

            var regTwo = MultiQubit.BasisVector(1, l);
            var regOne = new MultiQubit(Enumerable.Range(0, t).Select(i => Qubit.ClassicZero).ToArray());
            var input = new MultiQubit(regOne, regTwo);

            IQuantumState res = orderfinder.Transform(input);

            string inputStr = input.Print(regs);
            inputStr.Should().Be("+1.00|0>|1>");
            string outStr = res.Print(regs);
            // all first register options evenly divide 2^t and reduce to fractiosn
            // with a denominator of r = 2
            // in this case 0 and 32 over 2^6 equal 0 and 1/2
            // therefore answer is 2
            outStr.Should().Be("+0.50|0>|1>+0.50|0>|2>+0.50|32>|1>+-0.50|32>|2>");
        }
    }
}
