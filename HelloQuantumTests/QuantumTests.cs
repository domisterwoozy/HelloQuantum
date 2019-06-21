using FluentAssertions;
using HelloQuantum;
using System;
using System.Linq;
using System.Numerics;
using Xunit;

namespace HelloQuantumTests
{
    public static class ComplexAssertionExtensions
    {
        // need to get my test results to more precision to improve this
        public const double Precision = 0.000001;

        public static void ShouldBe(this Complex c, Complex target)
        {
            c.Real.Should().BeApproximately(target.Real, Precision);
            c.Imaginary.Should().BeApproximately(target.Imaginary, Precision);
        }
    }

    public class Tests
    {
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

            MultiQubit bell = new CGate(Gates.Not).Transform(control, target);
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
            MultiQubit bell = new CGate(Gates.Not).Transform(control, target);
            MultiQubit res = bell.ApplyGate(Gates.H, 0);

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
            MultiQubit bell = new CGate(Gates.Not).Transform(control, target);

            bell.TrueChance(0).Should().BeApproximately(0.5, ComplexAssertionExtensions.Precision);
            bell.TrueChance(1).Should().BeApproximately(0.5, ComplexAssertionExtensions.Precision);
        }

        [Fact]
        public void ThreeQubitFourier()
        {
            var amps = new[]
            {
                new Complex(12.500000, 10.000000),
                new Complex(-3.656854, 7.156854),
                new Complex(-12.500000, -8.000000),
                new Complex(-7.071068, -17.742641),
                new Complex(18.500000, -8.000000),
                new Complex(7.656854, -4.156854),
                new Complex(1.500000, -2.000000),
                new Complex(7.071068, -9.257359)
            };

            var expected = new[]
            {
                new Complex(3, -4),
                new Complex(0, 0),
                new Complex(1.5, 2),
                new Complex(-2, 0),
                new Complex(2, 2),
                new Complex(0, 1),
                new Complex(9, 1),
                new Complex(-1, 8)
            };

            var res = Fourier.ThreeBit(amps);
            for (int i = 0; i < amps.Length; i++)
            {
                res[i].ShouldBe(expected[i]);
            }

            var resN = Fourier.NBit(amps);
            for (int i = 0; i < amps.Length; i++)
            {
                resN[i].ShouldBe(expected[i]);
            }
        }

        [Fact]
        public void TwoQubitFourier()
        {
            var amps = new[]
            {
                new Complex(2.5, 6),
                new Complex(4.5, -1),
                new Complex(6.5, -4),
                new Complex(-1.5, 3)
            };

            var expected = new[]
            {
                new Complex(3, 1),
                new Complex(0, 4),
                new Complex(1.5, 0),
                new Complex(-2, 1)
            };

            var res = Fourier.TwoBit(amps);
            for (int i = 0; i < amps.Length; i++)
            {
                res[i].ShouldBe(expected[i]);
            }

            var resN = Fourier.NBit(amps);
            for (int i = 0; i < amps.Length; i++)
            {
                resN[i].ShouldBe(expected[i]);
            }
        }

        [Fact]
        public void TwoQubitFourierReal()
        {
            var amps = new[]
            {
                new Complex(2.5, 0),
                new Complex(1.5, -2),
                new Complex(6.5, 0),
                new Complex(1.5, 2)
            };

            var expected = new[]
            {
                new Complex(3, 0),
                new Complex(0, 0),
                new Complex(1.5, 0),
                new Complex(-2, 0)
            };

            var res = Fourier.TwoBit(amps);
            for (int i = 0; i < amps.Length; i++)
            {
                res[i].ShouldBe(expected[i]);
            }

            var resN = Fourier.NBit(amps);
            for (int i = 0; i < amps.Length; i++)
            {
                resN[i].ShouldBe(expected[i]);
            }
        }

        [Fact]
        public void FourBitFourier()
        {
            var amps = new[]
            {
                new Complex(1.000000,1.000000),
                new Complex(2.000001,2.000000),
                new Complex(3.000000,3.000000),
                new Complex(4.000000,4.000000),
                new Complex(0.000000,1.000000),
                new Complex(1.999995,2.000000),
                new Complex(3.000000,0.000000),
                new Complex(4.000000,4.000000),
                new Complex(1.000000,1.000000),
                new Complex(0.000003,2.000000),
                new Complex(3.000000,0.000000),
                new Complex(4.000000,4.000000),
                new Complex(1.000000,1.000000),
                new Complex(2.000001,2.000000),
                new Complex(3.000000,3.000000),
                new Complex(4.000000,4.000000)
            };
            var expected = new[]
            {
                new Complex(2.312500,2.125000),
                new Complex(0.115485,0.250500),
                new Complex(-0.025888,-0.088388),
                new Complex(0.047835,-0.087180),
                new Complex(-0.062500,-0.750000),
                new Complex(-0.047835,-0.212180),
                new Complex(0.150888,-0.088388),
                new Complex(-0.115485,0.375500),
                new Complex(-0.437500,-0.875000),
                new Complex(-0.115485,0.154830),
                new Complex(0.150888,0.088388),
                new Complex(-0.047835,-0.318150),
                new Complex(-1.062500,0.500000),
                new Complex(0.047835,-0.443150),
                new Complex(-0.025888,0.088388),
                new Complex(0.115485,0.279830)
            };

            var resN = Fourier.NBit(amps);
            for (int i = 0; i < amps.Length; i++)
            {
                resN[i].ShouldBe(expected[i]);
            }
        }

        [Fact]
        public void FiveBitFourier()
        {
            var expected = new[]
            {
                new Complex(38.000000,35.000000),
                new Complex(-17.895443,25.543153),
                new Complex(3.847761,6.477280),
                new Complex(-9.216936,13.129740),
                new Complex(2.585792,4.414208),
                new Complex(-8.247788,9.218005),
                new Complex(4.765361,-3.090400),
                new Complex(8.488227,13.216451),
                new Complex(-17.000000,9.000000),
                new Complex(-8.026356,-5.652166),
                new Complex(1.234634,-3.090400),
                new Complex(3.132755,-0.750147),
                new Complex(5.414208,1.414208),
                new Complex(-3.326046,4.413894),
                new Complex(2.152240,6.477280),
                new Complex(-7.630089,1.724720),
                new Complex(-6.000000,-13.000000),
                new Complex(11.824688,-4.481880),
                new Complex(-1.847757,8.008001),
                new Complex(-1.505026,-1.969436),
                new Complex(5.414208,-1.414208),
                new Complex(-3.610213,1.299210),
                new Complex(3.234639,0.605119),
                new Complex(-0.695021,-2.991710),
                new Complex(-0.000000,-11.000000),
                new Complex(11.140004,-2.684233),
                new Complex(2.765362,0.605119),
                new Complex(8.318391,-2.114216),
                new Complex(2.585792,1.585792),
                new Complex(13.641154,-7.155982),
                new Complex(5.847760,8.008001),
                new Complex(24.607698,-22.745403)
            };

            var amps = new[]
            {
                new Complex(73.999999,68.000000),
                new Complex(128.000004,128.000001),
                new Complex(96.000002,95.999999),
                new Complex(64.000035,64.000002),
                new Complex(32.000000,32.000000),
                new Complex(128.000002,128.000001),
                new Complex(95.999999,0.000002),
                new Complex(0.000093,63.999999),
                new Complex(32.000002,31.999999),
                new Complex(127.999996,128.000000),
                new Complex(96.000001,0.000003),
                new Complex(63.999840,63.999999),
                new Complex(-0.000000,32.000001),
                new Complex(128.000001,128.000000),
                new Complex(96.000001,95.999999),
                new Complex(64.000033,64.000000),
                new Complex(32.000001,32.000000),
                new Complex(3.695520,8.954560),
                new Complex(-0.828418,2.828415),
                new Complex(1.530718,-14.180803),
                new Complex(-34.000000,16.000000),
                new Complex(-1.530721,-10.180801),
                new Complex(4.828417,2.828416),
                new Complex(-3.695522,4.954561),
                new Complex(-14.000002,-27.999999),
                new Complex(-3.695521,12.016002),
                new Complex(4.828415,-2.828416),
                new Complex(-1.530718,-6.789762),
                new Complex(-2.000000,-24.000001),
                new Complex(1.530718,-2.789762),
                new Complex(-0.828417,-2.828417),
                new Complex(3.695521,8.016002)
            };

            var resN = Fourier.NBit(amps);
            for (int i = 0; i < amps.Length; i++)
            {
                resN[i].ShouldBe(expected[i]);
            }
        }

        [Fact]
        public void FiveBitFourierInverse()
        {
            var expected = new[]
            {
                new Complex(38.000000,35.000000),
                new Complex(-17.895443,25.543153),
                new Complex(3.847761,6.477280),
                new Complex(-9.216936,13.129740),
                new Complex(2.585792,4.414208),
                new Complex(-8.247788,9.218005),
                new Complex(4.765361,-3.090400),
                new Complex(8.488227,13.216451),
                new Complex(-17.000000,9.000000),
                new Complex(-8.026356,-5.652166),
                new Complex(1.234634,-3.090400),
                new Complex(3.132755,-0.750147),
                new Complex(5.414208,1.414208),
                new Complex(-3.326046,4.413894),
                new Complex(2.152240,6.477280),
                new Complex(-7.630089,1.724720),
                new Complex(-6.000000,-13.000000),
                new Complex(11.824688,-4.481880),
                new Complex(-1.847757,8.008001),
                new Complex(-1.505026,-1.969436),
                new Complex(5.414208,-1.414208),
                new Complex(-3.610213,1.299210),
                new Complex(3.234639,0.605119),
                new Complex(-0.695021,-2.991710),
                new Complex(-0.000000,-11.000000),
                new Complex(11.140004,-2.684233),
                new Complex(2.765362,0.605119),
                new Complex(8.318391,-2.114216),
                new Complex(2.585792,1.585792),
                new Complex(13.641154,-7.155982),
                new Complex(5.847760,8.008001),
                new Complex(24.607698,-22.745403)
            };

            var amps = new[]
            {
                new Complex(73.999999,68.000000),
                new Complex(128.000004,128.000001),
                new Complex(96.000002,95.999999),
                new Complex(64.000035,64.000002),
                new Complex(32.000000,32.000000),
                new Complex(128.000002,128.000001),
                new Complex(95.999999,0.000002),
                new Complex(0.000093,63.999999),
                new Complex(32.000002,31.999999),
                new Complex(127.999996,128.000000),
                new Complex(96.000001,0.000003),
                new Complex(63.999840,63.999999),
                new Complex(-0.000000,32.000001),
                new Complex(128.000001,128.000000),
                new Complex(96.000001,95.999999),
                new Complex(64.000033,64.000000),
                new Complex(32.000001,32.000000),
                new Complex(3.695520,8.954560),
                new Complex(-0.828418,2.828415),
                new Complex(1.530718,-14.180803),
                new Complex(-34.000000,16.000000),
                new Complex(-1.530721,-10.180801),
                new Complex(4.828417,2.828416),
                new Complex(-3.695522,4.954561),
                new Complex(-14.000002,-27.999999),
                new Complex(-3.695521,12.016002),
                new Complex(4.828415,-2.828416),
                new Complex(-1.530718,-6.789762),
                new Complex(-2.000000,-24.000001),
                new Complex(1.530718,-2.789762),
                new Complex(-0.828417,-2.828417),
                new Complex(3.695521,8.016002)
            };

            var inverseFourier = Fourier.FourierTransform(5).Inverse();
            var resN = inverseFourier.Transform(new MultiQubit(expected)).ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                resN[i].ShouldBe(amps[i]);
            }
        }        

        [Fact]
        public void PhaseStartSimpleTest()
        {

            var eigenvector = new Qubit(0, 1);
            var phaseTest = PhaseEstimator.GatePhaseEstimatorStart(Gates.S, 1);
            var res = phaseTest.Transform(new MultiQubit(new[]
            {
                eigenvector,
                Qubit.ClassicZero // t register set to zero
            }));

            // sanity check
            res.TwoNorm().Should().BeApproximately(1, ComplexAssertionExtensions.Precision);

            var resArr = res.ToArray();
            resArr[0].ShouldBe(0);
            resArr[1].ShouldBe(0);
            resArr[2].ShouldBe(ComplexExt.OneOverRootTwo);
            resArr[3].ShouldBe(Complex.ImaginaryOne * ComplexExt.OneOverRootTwo);
        }

        [Fact]
        public void PhaseStartTest()
        {

            var eigenvector = new Qubit(0, 1);
            var phaseTest = PhaseEstimator.GatePhaseEstimatorStart(Gates.S, 2);
            var res = phaseTest.Transform(new MultiQubit(new[]
            {
                eigenvector,
                Qubit.ClassicZero, Qubit.ClassicZero // t register set to zero
            }));

            // sanity check
            res.TwoNorm().Should().BeApproximately(1, ComplexAssertionExtensions.Precision);

            var resArr = res.ToArray();
            resArr[0].ShouldBe(0);
            resArr[1].ShouldBe(0);
            resArr[2].ShouldBe(0);
            resArr[3].ShouldBe(0);
            resArr[4].ShouldBe(0.5);
            resArr[5].ShouldBe(-0.5);
            resArr[6].ShouldBe(0.5 * Complex.ImaginaryOne);
            resArr[7].ShouldBe(-0.5 * Complex.ImaginaryOne);
        }

        [Fact]
        public void PhaseTestSimple()
        {
            var eigenvector = new Qubit(0, 1);
            var phaseTest = PhaseEstimator.GatePhaseEstimator(Gates.S, 1);
            var res = phaseTest.Transform(new MultiQubit(new[]
            {
                eigenvector,
                Qubit.ClassicZero // t register set to zero
            }));

            // sanity check
            res.TwoNorm().Should().BeApproximately(1, ComplexAssertionExtensions.Precision);

            var resArr = res.ToArray();
            resArr[0].ShouldBe(0);
            resArr[1].ShouldBe(0);
            resArr[2].ShouldBe(new Complex(0.5, 0.5));
            resArr[3].ShouldBe(new Complex(0.5, -0.5));
        }

        [Fact]
        public void PhaseTest()
        {
            var eigenvector = new Qubit(0, 1);
            var phaseTest = PhaseEstimator.GatePhaseEstimator(Gates.S, 4);
            var res = phaseTest.Transform(new MultiQubit(new[]
            {
                eigenvector,
                // t register set to zero
                Qubit.ClassicZero, Qubit.ClassicZero, Qubit.ClassicZero, Qubit.ClassicZero
            }));

            // sanity check
            res.TwoNorm().Should().BeApproximately(1, ComplexAssertionExtensions.Precision);

            // should be the same eigenvector
            res.TrueChance(0).Should().BeApproximately(1, ComplexAssertionExtensions.Precision);

            // eignenvalue is i -> i = e^(2pitheta) -> theta = 1/4
            res.TrueChance(4).Should().BeApproximately(0, ComplexAssertionExtensions.Precision); // 1/16 bit
            res.TrueChance(3).Should().BeApproximately(0, ComplexAssertionExtensions.Precision); // 1/8 bit
            res.TrueChance(2).Should().BeApproximately(1, ComplexAssertionExtensions.Precision); // 1/4 bit
            res.TrueChance(1).Should().BeApproximately(0, ComplexAssertionExtensions.Precision); // 1/2 bit
        }

        [Fact]
        public void PhaseTestTwo()
        {
            var eigenvector = new Qubit(ComplexExt.OneOverRootTwo, -ComplexExt.OneOverRootTwo);
            var phaseTest = PhaseEstimator.GatePhaseEstimator(Gates.X, 4);
            var res = phaseTest.Transform(new MultiQubit(new[]
            {
                eigenvector,
                // t register set to zero
                Qubit.ClassicZero, Qubit.ClassicZero, Qubit.ClassicZero, Qubit.ClassicZero
            }));

            // sanity check
            res.TwoNorm().Should().BeApproximately(1, ComplexAssertionExtensions.Precision);

            // should be the same eigenvector
            res.TrueChance(0).Should().BeApproximately(0.5, ComplexAssertionExtensions.Precision);

            // eignenvalue is -1 -> i = e^(2pitheta) -> theta = 1/2
            res.TrueChance(4).Should().BeApproximately(0, ComplexAssertionExtensions.Precision); // 1/16 bit
            res.TrueChance(3).Should().BeApproximately(0, ComplexAssertionExtensions.Precision); // 1/8 bit
            res.TrueChance(2).Should().BeApproximately(0, ComplexAssertionExtensions.Precision); // 1/4 bit
            res.TrueChance(1).Should().BeApproximately(1, ComplexAssertionExtensions.Precision); // 1/2 bit
        }
    }
}
