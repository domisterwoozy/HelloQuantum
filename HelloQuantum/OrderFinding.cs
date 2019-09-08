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

            long modMult(long y, long exp) => ((long)Math.Pow(x, exp) * y) % n;
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
}
