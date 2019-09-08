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

        /// <summary>
        /// The chance a certain configuration of qubits occurs
        /// </summary>
        double Chance(params (int qubitIndex, bool isOn)[] ps);
    }

    public static class QuantumStateExt
    {
        public static int NumQubits(this IQuantumState state) => (int)Math.Log(state.Dimension, 2);

        public class Register
        {
            public IEnumerable<int> QubitIndexes { get; set; }
        }

        public static string Print(this IQuantumState state, params Register[] registers)
        {
            var test = state.ToArray().Where(b => b.Magnitude > ComplexExt.Precision);
            StringBuilder sb = new StringBuilder();
            for (long i = 0; i < state.Dimension; i++)
            {
                var basis = new ComputationalBasis(i, state.NumQubits());
                var amp = state.GetAmplitude(basis);
                if (amp.Magnitude < ComplexExt.Precision)
                {
                    continue;
                }
                sb.Append($"+{(amp.Imaginary == 0 ? amp.Real.ToString("F2") : amp.ToString("F2"))}");

                var labels = basis.GetLabels();                
                foreach (var register in registers)
                {
                    var registerLabels = new List<bool>();
                    foreach (var index in register.QubitIndexes)
                    {
                        registerLabels.Add(labels[index]);
                    }
                    long regValue = ComputationalBasis.FromLabels(registerLabels.ToArray()).AmpIndex;
                    sb.Append($"|{regValue}>");
                }
            }
            return sb.ToString();
        }

        public static string Print(this IQuantumState state) => state.Print(
            Enumerable.Range(0, state.NumQubits())
            .Select(i => new Register
            {
                QubitIndexes = new[] { i }
            }).ToArray());
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

        public double Chance(params (int qubitIndex, bool isOn)[] ps)
        {
            if (ps.Length != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(ps));
            }
            return ps.Single().isOn ? TrueChance(0) : 1 - TrueChance(0);
        }
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

            return new ComputationalBasis(LongExt.FromBits(labels), labels.Length);
        }

        public static implicit operator ComputationalBasis(bool[] labels) => FromLabels(labels);
        public static implicit operator ComputationalBasis(string bitStr) => FromLabels(bitStr.ToBits());

        public bool[] GetLabels() => AmpIndex.ToBitsPad(NumQubits);
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

        public static MultiQubit BasisVector(long ampIndex, int numQubits)
        {
            var amps = new Complex[(long)Math.Pow(2, numQubits)];
            amps[ampIndex] = 1;
            return new MultiQubit(amps);
        }

        /// <summary>
        /// Tensor products two registers into one big quantum state.
        /// </summary>
        public MultiQubit(MultiQubit a, MultiQubit b)
        {
            NumQubits = a.NumQubits + b.NumQubits;

            amps = new Complex[a.Dimension * b.Dimension];

            for (long ia = 0; ia < a.Dimension; ia++)
            {
                bool[] aLabels = new ComputationalBasis(ia, a.NumQubits).GetLabels();

                for (long ib = 0; ib < b.Dimension; ib++)
                {
                    bool[] bLabels = new ComputationalBasis(ib, b.NumQubits).GetLabels();

                    var finalIndex = ComputationalBasis.FromLabels(aLabels.Concat(bLabels).ToArray());
                    amps[finalIndex.AmpIndex] = a.GetAmplitude(aLabels) * b.GetAmplitude(bLabels);
                }
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

        public double Chance(params (int qubitIndex, bool isOn)[] ps)
        {
            double res = 0;
            for (long i = 0; i < amps.LongLength; i++)
            {
                bool shortCircuit = false;
                bool[] lables = new ComputationalBasis(i, NumQubits).GetLabels();
                foreach (var (qubitIndex, isOn) in ps)
                {
                    if (lables[qubitIndex] != isOn)
                    {
                        shortCircuit = true;
                        break;
                    }
                }
                if (shortCircuit)
                {
                    continue;
                }

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
