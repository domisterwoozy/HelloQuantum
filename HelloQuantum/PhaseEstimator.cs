using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HelloQuantum
{
    public class PhaseEstimator
    {
        public static CompositeTransform GatePhaseEstimatorStart(IUnitaryTransform u, int t)
        {
            var phaseEstimator = new CompositeTransform(new IdentityTransform(u.NumQubits + t));
            foreach (int ti in Enumerable.Range(0, t))
            {
                phaseEstimator = phaseEstimator.Apply(Gates.H, u.NumQubits + ti);
                // apply you 2^(ti) times
                long j = (long)Math.Pow(2, ti);
                for (int num = 0; num < j; num++)
                {
                    phaseEstimator = phaseEstimator.ApplyControlled(
                        u, 
                        u.NumQubits + ti, 
                        Enumerable.Range(0, u.NumQubits).ToArray());
                }
            }

            return phaseEstimator;
        }

        /// <summary>
        /// First u.NumQubits qubits are the eigenvector, the rest is t register in
        /// significance descending order
        /// </summary>
        public static IUnitaryTransform GatePhaseEstimator(IUnitaryTransform u, int t)
        {
            long totalDimension = u.Dimension * (long)Math.Pow(2, t); // dimension of tensor product multiplies
            var phaseEstimator = GatePhaseEstimatorStart(u, t);

            // finally apply inverse qft to the t register
            return phaseEstimator.Apply(
                new PartialTransform(
                    totalDimension,
                    // phase trans assumes fourier does not do the swap and scales
                    Fourier.FourierTransform(t, false).Inverse(),
                    Enumerable.Range(u.NumQubits, t).ToArray()));
        }
    }
}
