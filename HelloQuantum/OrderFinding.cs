using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HelloQuantum
{
    public static class NumberTheoryTransforms
    {
        /// <summary>
        /// Takes a (hopefully linear function) from int to int and turns it into
        /// a complex unitary transform that reproducest he function when input with
        /// classical bits that represent the integer input.
        /// </summary>
        public static IUnitaryTransform FromFunction(Func<long, long> func, int numBits)
        {
            long dimension = (long)Math.Pow(2, numBits);
            var elements = new Complex[dimension, dimension];
            foreach (long i in LongExt.Range(0, dimension))
            {
                elements[func(i), i] = 1;
            }
            return new MultiGate(elements, (int)Math.Pow(numBits, 3)); // simulated with O(L^3) gates
        }

        /// <summary>
        /// U|input> = |x^j*input (mod N)>
        /// </summary>
        public static IUnitaryTransform ModMult(long x, long n, int j = 1)
        {
            int numBits = (int)Math.Ceiling(Math.Log(n, 2));            
            return FromFunction(modMult, numBits);

            long modMult(long y) => ((long)Math.Pow(x, j) * y) % n;
        }
    }
}
