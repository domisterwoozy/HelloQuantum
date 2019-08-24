using FluentAssertions;
using HelloQuantum;
using System;
using System.Linq;
using System.Numerics;
using Xunit;
using static HelloQuantum.QuantumStateExt;

namespace HelloQuantumTests
{
    public static class AssertionHelpers
    {
        // need to get my test results to more precision to improve this
        public const double Precision = 0.000001;

        public static void ShouldBe(this Complex c, Complex target)
        {
            c.Real.Should().BeApproximately(target.Real, Precision);
            c.Imaginary.Should().BeApproximately(target.Imaginary, Precision);
        }
    }

    public class BasicTests
    {
        [Fact]
        public void BellPrintTest()
        {
            Qubit control = new Qubit(ComplexExt.OneOverRootTwo, ComplexExt.OneOverRootTwo);
            Qubit target = new Qubit(1, 0);
            IQuantumState bell = new CTransform(Gates.Not).Transform(new MultiQubit(control, target));
            bell.Print().Should().Be("+0.71|0>|0>+0.71|1>|1>");
        }

        [Fact]
        public void TensorPrintTest()
        {
            var state = MultiQubit.BasisVector(14, 8);
            var stateOther = MultiQubit.BasisVector(5, 4);
            var compState = new MultiQubit(state, stateOther);
            compState.Print(new[]
            {
                new Register
                {
                    QubitIndexes = new[]{1,2,3,4,5,6,7}
                },
                new Register
                {
                    QubitIndexes = new[]{8,9,10,11}
                }
            }).Should().Be("+1.00|14>|5>");
        }

        [Fact]
        public void ComplexTests()
        {
            Complex[] res = new[] { Complex.One, Complex.One }.Normalize().ToArray();
            res[0].ShouldBe(ComplexExt.OneOverRootTwo);
            res[1].ShouldBe(ComplexExt.OneOverRootTwo);
        }

        [Fact]
        public void BasisTests()
        {
            var b1 = ComputationalBasis.FromLabels("101".ToBits());
            b1.AmpIndex.Should().Be(5);
            b1.GetLabels().Should().BeEquivalentTo("101".ToBits());

            var b2 = ComputationalBasis.FromLabels("001".ToBits());
            b2.AmpIndex.Should().Be(1);
            b2.GetLabels().Should().BeEquivalentTo("001".ToBits());

            var b3 = ComputationalBasis.FromLabels("1001".ToBits());
            b3.AmpIndex.Should().Be(9);
            b3.GetLabels().Should().BeEquivalentTo("1001".ToBits());

            var b4 = ComputationalBasis.FromLabels("00".ToBits());
            b4.AmpIndex.Should().Be(0);
            b4.GetLabels().Should().BeEquivalentTo("00".ToBits());
        }

        [Fact]
        public void BellTest()
        {
            Qubit control = new Qubit(ComplexExt.OneOverRootTwo, ComplexExt.OneOverRootTwo);
            Qubit target = new Qubit(1, 0);

            IQuantumState bell = new CTransform(Gates.Not).Transform(new MultiQubit(control, target));
            bell.GetAmplitude("00".ToBits()).ShouldBe(ComplexExt.OneOverRootTwo);
            bell.GetAmplitude("11".ToBits()).ShouldBe(ComplexExt.OneOverRootTwo);
            bell.GetAmplitude("01".ToBits()).ShouldBe(Complex.Zero);
            bell.GetAmplitude("10".ToBits()).ShouldBe(Complex.Zero);
        }

        /// <summary>
        /// https://cs.stackexchange.com/a/65380/106536
        /// </summary>
        [Fact]
        public void MultiQubitApplicationTest()
        {
            Qubit control = new Qubit(ComplexExt.OneOverRootTwo, ComplexExt.OneOverRootTwo);
            Qubit target = new Qubit(1, 0);
            IQuantumState bell = new CTransform(Gates.Not).Transform(new MultiQubit(control, target));
            IQuantumState res = new PartialTransform(4, Gates.H, new[] { 0 }).Transform(bell);

            res.GetAmplitude("00".ToBits()).ShouldBe(new Complex(1.0 / 2, 0));
            res.GetAmplitude("11".ToBits()).ShouldBe(-new Complex(1.0 / 2, 0));
            res.GetAmplitude("01".ToBits()).ShouldBe(new Complex(1.0 / 2, 0));
            res.GetAmplitude("10".ToBits()).ShouldBe(new Complex(1.0 / 2, 0));
        }

        [Fact]
        public void TrueChanceTests()
        {
            Qubit control = new Qubit(ComplexExt.OneOverRootTwo, ComplexExt.OneOverRootTwo);
            Qubit target = new Qubit(1, 0);
            IQuantumState bell = new CTransform(Gates.Not).Transform(new MultiQubit(control, target));

            bell.TrueChance(0).Should().BeApproximately(0.5, AssertionHelpers.Precision);
            bell.TrueChance(1).Should().BeApproximately(0.5, AssertionHelpers.Precision);
        }
        
        [Fact]
        public void MultiGateTest()
        {
            var elements = new Complex[2,2];
            elements[0, 0] = 1;
            elements[0, 1] = -1;
            elements[1, 1] = Complex.ImaginaryOne;
            var gate = new MultiGate(elements, 1);

            var res = gate.Transform(new MultiQubit(new[] { new Complex(0, 1), new Complex(1, 0) })).ToArray();
            res[0].ShouldBe(new Complex(-1, 1));
            res[1].ShouldBe(Complex.ImaginaryOne);
        }
    }
}
