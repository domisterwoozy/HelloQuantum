using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HelloQuantum
{
    public static class Fourier
    {
        public static IUnitaryTransform FourierTransform(int numQubits, bool scaleAndSwap = true)
        {
            var fourier = new CompositeTransform(new IdentityTransform(numQubits));
            for (int bitIndex = 0; bitIndex < numQubits; bitIndex++)
            {
                fourier = fourier.Apply(Gates.H, bitIndex);
                // success controlled phase gates from phase 2 to phase n - bitIndex
                // targeting bitIndex and with a control of 
                // off by 1 insanity coming up
                for (int phaseIndex = 2; phaseIndex + bitIndex - 1 < numQubits; phaseIndex++)
                {
                    fourier = fourier.ApplyControlled(
                        Gates.Phase(phaseIndex),
                        bitIndex + (phaseIndex - 1),
                        bitIndex);
                }
            }

            if (scaleAndSwap)
            {
                // scale time
                Gate scaleGate = Gates.I.Scale(ComplexExt.OneOverRootTwo);
                for (int bitIndex = 0; bitIndex < numQubits; bitIndex++)
                {
                    fourier = fourier.Apply(scaleGate, bitIndex);
                }

                // swap time
                // i think we just swap everything?
                for (int bitIndex = 0; bitIndex < numQubits / 2; bitIndex++)
                {
                    fourier = fourier.ApplyControlled(Gates.Not, bitIndex, numQubits - bitIndex - 1);
                    fourier = fourier.ApplyControlled(Gates.Not, numQubits - bitIndex - 1, bitIndex);
                    fourier = fourier.ApplyControlled(Gates.Not, bitIndex, numQubits - bitIndex - 1);
                }
            }         

            return fourier;
        }
    }
}
