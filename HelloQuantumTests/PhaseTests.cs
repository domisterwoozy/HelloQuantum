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
    public class PhaseTests
    {
        [Fact]
        public void PhaseStartSimpleTest()
        {

            var eigenvector = new Qubit(0, 1);
            var phaseTest = PhaseEstimator.GatePhaseEstimatorStart(Gates.S, 1);
            var res = phaseTest.Transform(new MultiQubit(new[]
            {
                Qubit.ClassicZero, // t register set to zero
                eigenvector                
            }));

            // sanity check
            res.TwoNorm().Should().BeApproximately(1, AssertionHelpers.Precision);

            // hand calced
            var resArr = res.ToArray();
            resArr[0].ShouldBe(0);
            resArr[1].ShouldBe(ComplexExt.OneOverRootTwo);
            resArr[2].ShouldBe(0);
            resArr[3].ShouldBe(Complex.ImaginaryOne * ComplexExt.OneOverRootTwo);
        }

        [Fact]
        public void PhaseStartTest()
        {
            var eigenvector = new Qubit(0, 1);
            var phaseTest = PhaseEstimator.GatePhaseEstimatorStart(Gates.S, 2);
            var res = phaseTest.Transform(new MultiQubit(new[]
            {
                Qubit.ClassicZero, Qubit.ClassicZero, // t register set to zero
                eigenvector
            }));

            // sanity check
            res.TwoNorm().Should().BeApproximately(1, AssertionHelpers.Precision);

            // hand calced
            var resArr = res.ToArray();
            resArr[0].ShouldBe(0);
            resArr[1].ShouldBe(0.5);
            resArr[2].ShouldBe(0);
            resArr[3].ShouldBe(0.5 * Complex.ImaginaryOne);
            resArr[4].ShouldBe(0);
            resArr[5].ShouldBe(-0.5);
            resArr[6].ShouldBe(0);
            resArr[7].ShouldBe(-0.5 * Complex.ImaginaryOne);
        }


        [Fact]
        public void PhaseTest()
        {
            var eigenvector = new Qubit(0, 1);
            var phaseTest = PhaseEstimator.GatePhaseEstimator(Gates.S, 4);
            var res = phaseTest.Transform(new MultiQubit(new[]
            {
                // t register set to zero
                Qubit.ClassicZero, Qubit.ClassicZero, Qubit.ClassicZero, Qubit.ClassicZero,
                eigenvector              
            }));

            // sanity check
            res.TwoNorm().Should().BeApproximately(1, AssertionHelpers.Precision);

            // should be the same eigenvector
            res.TrueChance(4).Should().BeApproximately(1, AssertionHelpers.Precision);

            // eignenvalue is i -> i = e^(2pitheta) -> theta = 1/4
            res.TrueChance(0).Should().BeApproximately(0, AssertionHelpers.Precision); // 1/2 bit
            res.TrueChance(1).Should().BeApproximately(1, AssertionHelpers.Precision); // 1/4 bit
            res.TrueChance(2).Should().BeApproximately(0, AssertionHelpers.Precision); // 1/8 bit
            res.TrueChance(3).Should().BeApproximately(0, AssertionHelpers.Precision); // 1/16 bit
        }

        [Fact]
        public void PhaseTestTwo()
        {
            var eigenvector = new Qubit(ComplexExt.OneOverRootTwo, -ComplexExt.OneOverRootTwo);
            var phaseTest = PhaseEstimator.GatePhaseEstimator(Gates.X, 4);
            var res = phaseTest.Transform(new MultiQubit(new[]
            {
                // t register set to zero
                Qubit.ClassicZero, Qubit.ClassicZero, Qubit.ClassicZero, Qubit.ClassicZero,
                eigenvector,               
            }));

            // sanity check
            res.TwoNorm().Should().BeApproximately(1, AssertionHelpers.Precision);

            // should be the same eigenvector
            res.TrueChance(4).Should().BeApproximately(0.5, AssertionHelpers.Precision);

            // eignenvalue is -1 -> i = e^(2pitheta) -> theta = 1/2
            res.TrueChance(0).Should().BeApproximately(1, AssertionHelpers.Precision); // 1/2 bit
            res.TrueChance(1).Should().BeApproximately(0, AssertionHelpers.Precision); // 1/4 bit
            res.TrueChance(2).Should().BeApproximately(0, AssertionHelpers.Precision); // 1/8 bit
            res.TrueChance(3).Should().BeApproximately(0, AssertionHelpers.Precision); // 1/16 bit
        }



        [Fact]
        public void PhaseEstComplexity()
        {
            // complexity = (sum(2^n) from 0 to n) + n + n*(n+1) / 2
            // todo: add back swaps
            PhaseEstimator.GatePhaseEstimator(Gates.X, 2).NumGates.Should().Be(3 + 2 + 1 + 2 + 3 * 1);
            PhaseEstimator.GatePhaseEstimator(Gates.X, 3).NumGates.Should().Be(6 + 3 + 1 + 2 + 4 + 3 * 1);
            PhaseEstimator.GatePhaseEstimator(Gates.X, 4).NumGates.Should().Be(10 + 4 + 1 + 2 + 4 + 8 + 3 * 2);
            PhaseEstimator.GatePhaseEstimator(Gates.X, 5).NumGates.Should().Be(15 + 5 + 1 + 2 + 4 + 8 + 16 + 3 * 2);
        }
    }
}
