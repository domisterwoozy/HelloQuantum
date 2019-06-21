using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HelloQuantum
{
    public static class Fourier
    {
        public static Complex[] TwoBit(Complex[] amps)
        {
            if (amps.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(amps));
            }

            var qubits = new MultiQubit(amps);
            qubits = qubits.ApplyGate(Gates.H, 0);
            qubits = qubits.ApplyControlledGate(Gates.Phase(2), 1, 0);
            qubits = qubits.ApplyGate(Gates.H, 1);

            // post processing, scale and swaps
            Gate scaleGate = Gates.I.Scale(ComplexExt.OneOverRootTwo);
            qubits = qubits.ApplyGate(scaleGate, 0).ApplyGate(scaleGate, 1);
            // this is how you swap two quibits
            qubits = qubits.ApplyControlledGate(Gates.Not, 0, 1);
            qubits = qubits.ApplyControlledGate(Gates.Not, 1, 0);
            qubits = qubits.ApplyControlledGate(Gates.Not, 0, 1);

            return qubits.ToArray();
        }

        public static Complex[] ThreeBit(Complex[] amps)
        {
            if (amps.Length != 8)
            {
                throw new ArgumentOutOfRangeException(nameof(amps));
            }

            var qubits = new MultiQubit(amps);
            qubits = qubits.ApplyGate(Gates.H, 0);
            qubits = qubits.ApplyControlledGate(Gates.Phase(2), 1, 0);
            qubits = qubits.ApplyControlledGate(Gates.Phase(3), 2, 0);
            qubits = qubits.ApplyGate(Gates.H, 1);
            qubits = qubits.ApplyControlledGate(Gates.Phase(2), 2, 1);
            qubits = qubits.ApplyGate(Gates.H, 2);

            // post processing, scale and swaps
            Gate scaleGate = Gates.I.Scale(ComplexExt.OneOverRootTwo);
            qubits = qubits.ApplyGate(scaleGate, 0).ApplyGate(scaleGate, 1).ApplyGate(scaleGate, 2);
            // this is how you swap two quibits
            qubits = qubits.ApplyControlledGate(Gates.Not, 0, 2);
            qubits = qubits.ApplyControlledGate(Gates.Not, 2, 0);
            qubits = qubits.ApplyControlledGate(Gates.Not, 0, 2);

            return qubits.ToArray();
        }

        public static Complex[] NBit(Complex[] amps)
        {
            int n = (int)Math.Log(amps.LongLength, 2);

            var qubits = new MultiQubit(amps);
            return FourierTransform(n).Transform(qubits).ToArray();
        }

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
