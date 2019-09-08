using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static HelloQuantum.QuantumStateExt;

namespace HelloQuantum
{
    /// <summary>
    /// Has two registers. Register 1 has t qubits. Register 2 has l qubits.
    /// Both are in descending significance order
    /// </summary>
    public class PhaseEstimator
    {
        public static CompositeTransform GetPhaseHadamar(int t, int l)
        {
            var phaseEstimator = new CompositeTransform(new IdentityTransform(t + l));

            foreach (int ti in Enumerable.Range(0, t))
            {
                phaseEstimator = phaseEstimator.Apply(Gates.H, ti);
            }

            return phaseEstimator;
        }

        public static CompositeTransform GatePhaseEstimatorStart(IUnitaryTransform u, int t)
        {
            var phaseEstimator = GetPhaseHadamar(t, u.NumQubits);

            foreach (int ti in Enumerable.Range(0, t))
            {
                // apply u 2^(ti) times
                long j = (long)Math.Pow(2, ti);
                phaseEstimator = phaseEstimator.ApplyControlled(
                    u,
                    t - 1 - ti,
                    Enumerable.Range(t, u.NumQubits).ToArray(),
                    j);
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
                    Fourier.FourierTransform(t, false, true).Inverse(),
                    Enumerable.Range(0, t).ToArray()));
        }
    }
}
