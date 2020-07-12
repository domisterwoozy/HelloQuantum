using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HelloQuantum
{
    public static class Grover
    {
        /// <summary>
        /// The blackBoxFunc represents a search problem, where the goal is to find the inputs that
        /// result in true. This function turns the classical search problem into a quantum transformation
        /// by either flipping the input when the function is true, or leaving it unchanged when it is false.
        /// |x> -> (-1)^f(x) |x>
        /// </summary>
        public static IUnitaryTransform GetOracle(bool[] blackBoxFunc)
        {
            long dim = blackBoxFunc.LongLength;
            var elements = new Complex[dim, dim];
            foreach (long i in LongExt.Range(0, dim))
            {
                elements[i, i] = blackBoxFunc[i] ? -1 : 1;
            }
            return new MultiGate(elements, 1);
        }

        public static CompositeTransform GetGroverDiffusionOperator(bool[] blackBoxFunc)
        {
            int n = blackBoxFunc.LongLength.BitsCeiling();
            // 1. oracle
            var grover = new CompositeTransform(GetOracle(blackBoxFunc));

            // 2. hadamar all bits
            foreach (int i in Enumerable.Range(0, n))
            {
                grover = grover.Apply(Gates.H, i);
            }

            // 3. TODO: phase flip every basis except |0> (how do i do this with basic gates?? going to do a custom MultiGate for now)
            // from mike and ike: 'can be implemented using the techniques of section 4.3 using O(n) gates
            long dim = blackBoxFunc.LongLength;
            var elements = new Complex[dim, dim];
            foreach (long i in LongExt.Range(0, dim))
            {
                elements[i, i] = i == 0 ? 1 : -1;
            }
            grover = grover.Apply(new MultiGate(elements, n));

            //4. hadamar all bits again
            foreach (int i in Enumerable.Range(0, n))
            {
                grover = grover.Apply(Gates.H, i);
            }

            return grover;
        }

        public static CompositeTransform GetGroverTransform(bool[] blackBoxFunc)
        {
            long numStates = blackBoxFunc.LongLength;
            int numQubits = numStates.BitsCeiling();
            var result = new CompositeTransform(new IdentityTransform(numQubits));
            var groverIteration = GetGroverDiffusionOperator(blackBoxFunc);
            int iterationCount = (int)Math.Round(Math.PI * Math.Sqrt(numStates) / 4);
            foreach(int i in Enumerable.Range(0, iterationCount))
            {
                result = result.Apply(groverIteration);
            }
            return result;        
        }

        public static long Find(bool[] blackBoxFunc)
        {
            long numStates = blackBoxFunc.LongLength;
            int numQubits = numStates.BitsCeiling();
            var input = new MultiQubit(Enumerable.Range(0, numQubits).Select(i => Qubit.ClassicZero).ToArray());
            var reg = new QuantumStateExt.Register { QubitIndexes = Enumerable.Range(0, numQubits) };
            var grover = GetGroverTransform(blackBoxFunc);
            var sim = new QuantumSim(grover, reg);
            while (true)
            {
                long res = sim.Simulate(input)[reg];
                if (blackBoxFunc[res])
                {
                    return res;
                }
            }
            
        }
    }
}
