using System;
using System.Collections.Generic;
using System.Text;

namespace HelloQuantum
{
    /// <summary>
    /// U|input> = |x*input (mod N)>
    /// </summary>
    public class ModMult : IUnitaryTransform
    {
        public long Dimension { get; }
        public int NumQubits { get; }

        // need to see how much this is. prob depends on number of bits
        public int NumGates { get; } = 1;

        public long X { get; }
        public long N { get; }

        public ModMult(long x, long n)
        {
            X = x;
            N = n;

            NumQubits = (int)Math.Ceiling(Math.Log(n, 2)); // need ceiling to capture N fully
            Dimension = (long)Math.Pow(2, NumQubits);               
        }        

        public IQuantumState Transform(IQuantumState input)
        {
            // crap this prob needs a matrix
            throw new NotImplementedException();
        }

        public IUnitaryTransform Inverse()
        {
            throw new NotImplementedException();
        }
    }
}
