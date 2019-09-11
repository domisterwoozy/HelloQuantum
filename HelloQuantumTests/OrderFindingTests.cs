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

        [Fact]
        public void OrderFindSimTest()
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

            var sim = new QuantumSim(orderfinder, regs);
            var res = sim.Simulate(input);

            res.First().Value.Should().BeOneOf(0, 32);
        }

        [Fact]
        public void OrderFindFinalTest()
        {
            OrderFinder.Calculate(2, 3).Should().Be(2);
            OrderFinder.Calculate(3, 4).Should().Be(2);
        }

        [Fact]
        public void OrderFindFinalLongTest()
        {
            OrderFinder.Calculate(2, 5).Should().Be(4);
        }

        [Fact]
        public void FractionTests()
        {
            FractionHelpers.GetContinuedFractionCoeffs(415, 93).Should().BeEquivalentTo(new[] { 4, 2, 6, 7 });
            FractionHelpers.GetContinuedFractionCoeffs(31, 13).Should().BeEquivalentTo(new[] { 2, 2, 1, 1, 2 });
            FractionHelpers.GetContinuedFractionCoeffs(1536, 2048).Should().BeEquivalentTo(new[] { 0, 1, 3 });

            FractionHelpers.GetContinuedFractionCoeffs(117, 500).Should().BeEquivalentTo(new[] { 0, 4, 3, 1, 1, 1, 10 });          
        }

        [Fact]
        public void FractionTestTwo()
        {
            FractionHelpers.GetFraction(new long[] { 4, 3, 2 }).Should().Be((30, 7));

            // on the way to 0.234 (117/ 500) we should get to 11/47
            FractionHelpers.GetContinuedFractionSequence(117, 500).Should().Contain((11, 47));
        }
    }
}
