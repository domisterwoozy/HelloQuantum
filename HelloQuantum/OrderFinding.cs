using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using static HelloQuantum.QuantumStateExt;

namespace HelloQuantum
{
    public static class NumberTheoryTransforms
    {
        /// <summary>
        /// Takes a (hopefully linear function) from int to int and turns it into
        /// a complex unitary transform that reproducest he function when input with
        /// classical bits that represent the integer input.
        /// </summary>
        public static IUnitaryTransform FromFunction(
            Func<long, long> func,
            int numBits, 
            Func<long, long, long> powVersion = null)
        {
            long dimension = (long)Math.Pow(2, numBits);
            var elements = new Complex[dimension, dimension];
            foreach (long i in LongExt.Range(0, dimension))
            {
                elements[func(i), i] = 1;
            }
            return new MultiGate(
                elements, 
                (int)Math.Pow(numBits, 3), // simulated with O(L^3) gates
                exp => FromFunction(
                    input => powVersion(input, exp),
                    numBits, // same input bits
                    powVersion)); // same method for powering
        }

        /// <summary>
        /// U|input> = |x^j*input (mod N)>
        /// </summary>
        public static IUnitaryTransform ModMult(long x, long n)
        {
            int numBits = n.BitsCeiling();            
            return FromFunction(y => modMult(y, 1), numBits, modMult);

            long modMult(long y, long exp) => ((long)BigInteger.ModPow(x, exp, n) * y) % n;
        }
    }

    /// <summary>
    /// for an x and N, find r such that
    /// x^r = 1 (mod N)
    /// </summary>
    public static class OrderFindingTransform
    {
        public static int GetPercision(long n, double failChance = 0.45) 
            => 2 * n.BitsCeiling() + 1 + (int)Math.Log(2 + 1 / (2 * failChance), 2);

        public static IEnumerable<Register> Registers(int t, int l) => new[]
        {
            new Register
            {
                QubitIndexes = Enumerable.Range(0, t)
            },
            new Register
            {
                QubitIndexes = Enumerable.Range(t, l)
            }           
        };

        /// <summary>
        /// Register 1: first L qubits - pass in |1> (first basis vector)
        /// Register 2: last t qubits - pass in all zeros (i think N = 2^L)
        /// 
        /// Solves for r where x^r = 1 (mod n)
        /// </summary>
        public static IUnitaryTransform Get(long x, long n, int t)
        {
            return PhaseEstimator.GatePhaseEstimator(NumberTheoryTransforms.ModMult(x, n), t);
        }

        /// <summary>
        /// Register 1: first L qubits - pass in |1> (first basis vector)
        /// Register 2: last t qubits - pass in all zeros (i think N = 2^L)
        /// </summary>
        public static IUnitaryTransform GetStart(long x, long n, int t)
        {
            return PhaseEstimator.GatePhaseEstimatorStart(NumberTheoryTransforms.ModMult(x, n), t);
        }
    }

    public static class FractionHelpers
    {
        public static IEnumerable<long> GetContinuedFractionCoeffs(long num, long denom)
        {
            if (denom == 0)
            {
                throw new ArgumentException(nameof(denom));
            }

            if (num == 0)
            {
                return new long[] { 0 };
            }

            if (num < denom)
            {
                return new long[] { 0 }.Concat(GetContinuedFractionCoeffs(denom, num));
            }

            long wholePart = num / denom;
            long remainder = num % denom;

            if (remainder == 0)
            {
                return new[] { wholePart };
            }

            return new[] { wholePart }.Concat(GetContinuedFractionCoeffs(denom, remainder));
        }

        /// <summary>
        /// Converts a continued fraction coefficient sequence into a single
        /// simplified fraction.
        /// </summary>
        public static (long num, long denom) GetFraction(IEnumerable<long> coeffs)
        {
            long currNum = coeffs.Last();
            long currDenom = 1;

            foreach (var next in coeffs.Reverse().Skip(1))
            {
                // do 1 / yourself than add next
                long newDenom = currNum;
                long newNum = currDenom;
                newNum += next * newDenom;

                currNum = newNum;
                currDenom = newDenom;
            }

            return (currNum, currDenom);
        }

        /// <summary>
        /// Gets a sequence of fractions that converge towards a specified fraction.
        /// This sequence converges in logN steps
        /// </summary>
        public static IEnumerable<(long num, long denom)> GetContinuedFractionSequence(long num, long denom)
        {
            var coeffs = GetContinuedFractionCoeffs(num, denom).ToArray();
            return Enumerable.Range(1, coeffs.Length).Select(i => GetFraction(coeffs.Take(i)));
        }
    }

    public static class OrderFinder
    {
        public static long Calculate(long x, long n)
        {
            int l = n.BitsCeiling();
            int t = OrderFindingTransform.GetPercision(n);

            var regs = OrderFindingTransform.Registers(t, l).ToArray();

            IUnitaryTransform orderfinder = OrderFindingTransform.Get(x, n, t);

            var regTwo = MultiQubit.BasisVector(1, l);
            var regOne = new MultiQubit(Enumerable.Range(0, t).Select(i => Qubit.ClassicZero).ToArray());
            var input = new MultiQubit(regOne, regTwo);

            long denom = (long)Math.Pow(2, t);
            while (true)
            {
                QuantumSim sim = new QuantumSim(orderfinder, regs);
                IDictionary<Register, long> res = sim.Simulate(input);
                long regValue = res.First().Value;
                // can't do anything if we get zero, cant reduce
                if (regValue == 0)
                {
                    continue;
                }
                foreach (var (_, rCandidate) in FractionHelpers.GetContinuedFractionSequence(regValue, denom))
                {
                    if ((long)Math.Pow(x, rCandidate) % n == 1)
                    {                        
                        return rCandidate;
                    }
                }
            }
        }
    }
}
