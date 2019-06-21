using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HelloQuantum
{
    public interface IQuantumState : IEnumerable<Complex>
    {
        long Dimension { get; }

        /// <summary>
        /// The amplitudes in the standard computational basis.
        /// </summary>
        Complex GetAmplitude(ComputationalBasis basis);

        double TrueChance(int qubitIndex);
    }

    public struct Qubit : IQuantumState
    {
        public static readonly Qubit ClassicOne = new Qubit(0, 1);
        public static readonly Qubit ClassicZero = new Qubit(1, 0);

        /// <summary>
        /// Zero amplittude in the computational basis
        /// </summary>
        public Complex AmpZero { get; }
        /// <summary>
        /// One amplittude in the computation basis
        /// </summary>
        public Complex AmpOne { get; }

        public long Dimension => 2;

        public Qubit(Complex zero, Complex one)
        {
            if (!new[] { zero, one }.IsNormalized())
            {
                //throw new InvalidOperationException("Big booboo");
            }

            AmpOne = one;
            AmpZero = zero;
        }

        public Complex GetAmplitude(ComputationalBasis basis)
        {
            switch (basis.AmpIndex)
            {
                case 0: return AmpZero;
                case 1: return AmpOne;
                default:
                    throw new ArgumentOutOfRangeException(nameof(basis));
            }
        }

        public IEnumerator<Complex> GetEnumerator() => new List<Complex> { AmpZero, AmpOne }.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public double TrueChance(int qubitIndex) => (AmpOne * AmpOne).Magnitude;
    }

    public struct ComputationalBasis
    {
        public long AmpIndex { get; }
        public int NumQubits { get; }

        public ComputationalBasis(long index, int numQubits)
        {
            AmpIndex = index;
            NumQubits = numQubits;
        }

        public static ComputationalBasis FromLabels(bool[] labels)
        {
            if (labels.Length > 60)
            {
                throw new ArgumentOutOfRangeException("Too many labels");
            }

            long index = 0;
            int k = 0;
            foreach (var item in labels)
            {
                k++;
                if (!item) continue;
                index += (long)Math.Pow(2, labels.Length - k);
            }

            return new ComputationalBasis(index, labels.Length);
        }

        public static implicit operator ComputationalBasis(bool[] labels) => FromLabels(labels);
        public static implicit operator ComputationalBasis(string bitStr) => FromLabels(bitStr.ToBits());

        public bool[] GetLabels()
        {
            bool[] bits = ToBinary(AmpIndex).ToArray();
            // need to pad with zeros to get the correct number of labels
            return new bool[NumQubits - bits.Length].Concat(bits).ToArray();
        }

        public static IEnumerable<bool> ToBinary(long n)
        {
            if (n < 2)
            {
                return new[] { n == 1 };
            }

            var divisor = n / 2;
            var remainder = n % 2;

            return ToBinary(divisor).Concat(new[] { remainder == 1 });
        }
    }

    public class MultiQubit : IQuantumState
    {
        public int NumQubits { get; }
        public long Dimension => (long)Math.Pow(2, NumQubits);

        private readonly Complex[] amps;

        public MultiQubit(params Qubit[] qubits)
        {
            NumQubits = qubits.Length;

            // now this is why its hard to simulate a quantum computer
            // you need to keep track of a shitload of amplitudes 2^N
            amps = new Complex[(long)Math.Pow(2, qubits.Length)];

            for (long i = 0; i < amps.LongLength; i++)
            {
                Complex amp = Complex.One;
                int bitNum = 0;
                foreach (bool bit in new ComputationalBasis(i, NumQubits).GetLabels())
                {
                    Qubit q = qubits[bitNum];
                    amp *= bit ? q.AmpOne : q.AmpZero;
                    bitNum++;
                }
                amps[i] = amp;
            }

            if (!amps.IsNormalized())
            {
                //throw new InvalidOperationException("Big booboo");
            }
        }

        public MultiQubit(Complex[] amps)
        {
            if (!amps.IsNormalized())
            {
                //throw new ArgumentException("Amplitudes must be normalized");
            }

            if (!amps.LongLength.IsPowerTwo())
            {
                throw new ArgumentException("Multi quibit amp must number a power of two");
            }

            NumQubits = (int)Math.Log(amps.LongLength, 2);
            this.amps = amps;
        }

        public Complex GetAmplitude(ComputationalBasis basis)
        {
            if (basis.NumQubits != NumQubits)
            {
                throw new ArgumentException("Incorrect size of basis vector specified");
            }

            return amps[basis.AmpIndex];
        }

        /// <summary>
        /// Applies a single qubit gate to a specified qubit
        /// </summary>
        public MultiQubit ApplyGate(Gate g, int qubitIndex)
        {
            Complex[] newAmps = new Complex[amps.LongLength];

            // loop through every amplitude and apply the gate to the single qubit
            // this represents well how a change to one qubit actually affects all 2^N amps
            for (long i = 0; i < amps.LongLength; i++)
            {
                // pass the classical bit represented by this basis through the gate and see the output
                bool[] lables = new ComputationalBasis(i, NumQubits).GetLabels();
                bool bit = lables[qubitIndex];
                Qubit res = g.Transform(bit ? Qubit.ClassicOne : Qubit.ClassicZero);
                // += is needed b/c this amp might have already been written too
                newAmps[i] += amps[i] * (bit ? res.AmpOne : res.AmpZero);
                // the gate mixed this basis to the other corresponding basis with a diff label at qubitIndex
                lables[qubitIndex] = !lables[qubitIndex];
                long mixedAmpIndex = ComputationalBasis.FromLabels(lables).AmpIndex;
                // important to still multiply by amps[i] b/c thats the original amp were multiplying by
                newAmps[mixedAmpIndex] += amps[i] * (bit ? res.AmpZero : res.AmpOne);
            }

            return new MultiQubit(newAmps); // pretty sure we don't need to renomralize here
        }

        /// <summary>
        /// Applies a controlled qubit gate to a specified control/target
        /// </summary>
        public MultiQubit ApplyControlledGate(Gate g, int controlQubitIndex, int targetQubitIndex)
        {
            Complex[] newAmps = new Complex[amps.LongLength];

            // loop through every amplitude and apply the gate to the single qubit
            // this represents well how a change to one qubit actually affects all 2^N amps
            for (long i = 0; i < amps.LongLength; i++)
            {
                // pass the classical bit represented by this basis through the gate and see the output
                bool[] lables = new ComputationalBasis(i, NumQubits).GetLabels();
                bool controlBit = lables[controlQubitIndex];
                bool targetBit = lables[targetQubitIndex];
                // this old amp can now get smeared out among 4 diff basis
                // need to extend the rest of this method to n instead of 4
                IQuantumState res = new CGate(g).Transform(
                    controlBit ? Qubit.ClassicOne : Qubit.ClassicZero,
                    targetBit ? Qubit.ClassicOne : Qubit.ClassicZero);

                Complex amp00 = res.GetAmplitude("00".ToBits());
                lables[controlQubitIndex] = false;
                lables[targetQubitIndex] = false;
                long index00 = ComputationalBasis.FromLabels(lables).AmpIndex;
                newAmps[index00] += amps[i] * amp00;

                Complex amp01 = res.GetAmplitude("01".ToBits());
                lables[controlQubitIndex] = false;
                lables[targetQubitIndex] = true;
                long index01 = ComputationalBasis.FromLabels(lables).AmpIndex;
                newAmps[index01] += amps[i] * amp01;

                Complex amp10 = res.GetAmplitude("10".ToBits());
                lables[controlQubitIndex] = true;
                lables[targetQubitIndex] = false;
                long index10 = ComputationalBasis.FromLabels(lables).AmpIndex;
                newAmps[index10] += amps[i] * amp10;

                Complex amp11 = res.GetAmplitude("11".ToBits());
                lables[controlQubitIndex] = true;
                lables[targetQubitIndex] = true;
                long index11 = ComputationalBasis.FromLabels(lables).AmpIndex;
                newAmps[index11] += amps[i] * amp11;
            }

            return new MultiQubit(newAmps); // pretty sure we don't need to renomralize here
        }

        public double TrueChance(int qubitIndex)
        {
            double res = 0;
            for (long i = 0; i < amps.LongLength; i++)
            {
                bool[] lables = new ComputationalBasis(i, NumQubits).GetLabels();
                bool bit = lables[qubitIndex];
                if (!bit) continue;
                res += (amps[i] * amps[i]).Magnitude;
            }
            return res;
        }

        public MultiQubit CollapseQubit(int qubitIndex, bool classicalValue)
        {
            Complex[] newAmps = new Complex[amps.LongLength];


            for (long i = 0; i < amps.LongLength; i++)
            {
                bool[] lables = new ComputationalBasis(i, NumQubits).GetLabels();
                bool bit = lables[qubitIndex];
                if (bit != classicalValue) continue;
                newAmps[i] = amps[i];
            }

            return new MultiQubit(newAmps); // pretty sure we must normalize here
        }

        public IEnumerator<Complex> GetEnumerator()
        {
            List<Complex> ret = new List<Complex>();
            for (long i = 0; i < amps.LongLength; i++)
            {
                // pass the classical bit represented by this basis through the gate and see the output
                bool[] lables = new ComputationalBasis(i, NumQubits).GetLabels();
                ret.Add(GetAmplitude(lables));
            }
            return ret.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        
    }
}
