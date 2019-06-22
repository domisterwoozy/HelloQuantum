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
    public class FourierTests
    {
        [Fact]
        public void FourierComplexity()
        {
            // complexity = n*(n+1) / 2
            Fourier.FourierTransform(2, false).NumGates.Should().Be(3);
            Fourier.FourierTransform(3, false).NumGates.Should().Be(6);
            Fourier.FourierTransform(4, false).NumGates.Should().Be(10);
            Fourier.FourierTransform(5, false).NumGates.Should().Be(15);
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

            var fourier = Fourier.FourierTransform(3);
            var inverseFourier = fourier.Inverse();

            var resN = inverseFourier.Transform(new MultiQubit(expected)).ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                resN[i].ShouldBe(amps[i]);
            }

            var resTwo = fourier.Transform(new MultiQubit(amps)).ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                resTwo[i].ShouldBe(expected[i]);
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

            var inverseFourier = Fourier.FourierTransform(2).Inverse();
            var resN = inverseFourier.Transform(new MultiQubit(expected)).ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                resN[i].ShouldBe(amps[i]);
            }

            var fourier = Fourier.FourierTransform(2);
            var resTwo = fourier.Transform(new MultiQubit(amps)).ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                resTwo[i].ShouldBe(expected[i]);
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

            var fourier = Fourier.FourierTransform(2);
            var inverseFourier = fourier.Inverse();

            var resN = inverseFourier.Transform(new MultiQubit(expected)).ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                resN[i].ShouldBe(amps[i]);
            }

            var resTwo = fourier.Transform(new MultiQubit(amps)).ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                resTwo[i].ShouldBe(expected[i]);
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

            var fourier = Fourier.FourierTransform(4);
            var inverseFourier = fourier.Inverse();

            var resN = inverseFourier.Transform(new MultiQubit(expected)).ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                resN[i].ShouldBe(amps[i]);
            }

            var resTwo = fourier.Transform(new MultiQubit(amps)).ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                resTwo[i].ShouldBe(expected[i]);
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

            var fourier = Fourier.FourierTransform(5);
            var inverseFourier = fourier.Inverse();

            var resN = inverseFourier.Transform(new MultiQubit(expected)).ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                resN[i].ShouldBe(amps[i]);
            }

            var resTwo = fourier.Transform(new MultiQubit(amps)).ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                resTwo[i].ShouldBe(expected[i]);
            }
        }
    }
}
