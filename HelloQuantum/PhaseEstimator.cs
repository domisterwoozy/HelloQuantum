using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HelloQuantum
{
    public class PhaseEstimator
    {
        /// <summary>
        /// First u.NumQubits qubits are the eigenvector, the rest is t register in
        /// significance ascending order
        /// </summary>
        public static IUnitaryTransform GatePhaseEstimator(Gate u, int t)
        {
            long totalDimension = u.Dimension * (long)Math.Pow(2, t); // dimension of tensor product multiplies
            var phaseEstimator = new CompositeTransform(new IdentityTransform(u.NumQubits + t));
            foreach (int ti in Enumerable.Range(0, t))
            {
                phaseEstimator = phaseEstimator.Apply(Gates.H, u.NumQubits + ti);
                // apply you 2^(ti) times
                long j = (long)Math.Pow(2, ti);
                for (int num = 0; num < j; num++)
                {
                    phaseEstimator = phaseEstimator.ApplyControlled(u, u.NumQubits + ti, 0);
                }
            }

            // finally apply inverse qft to the t register
            return phaseEstimator.Apply(
                new PartialTransform(
                    totalDimension,
                    Fourier.FourierTransform(t).Inverse(),
                    Enumerable.Range(u.NumQubits, t).ToArray()));
        }

        public static IUnitaryTransform GatePhaseEstimator(IUnitaryTransform u, int t)
        {
            throw new NotImplementedException();
        }
    }
}
