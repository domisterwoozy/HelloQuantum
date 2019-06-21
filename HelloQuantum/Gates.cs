using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HelloQuantum
{
    public interface IUnitaryTransform
    {
        /// <summary>
        /// The number of dimensions in the vector space this transformation acts
        /// 2^NumQubits
        /// </summary>
        long Dimension { get; }
        /// <summary>
        /// The amount of qubits in a circuit represented by this transform
        /// </summary>
        int NumQubits { get; }
        IQuantumState Transform(IQuantumState input);
        IUnitaryTransform Inverse();
    }

    /// <summary>
    /// A single quibit gate. No enforcement on unitary nature
    /// </summary>
    public struct Gate : IUnitaryTransform
    {
        public Complex A { get; }
        public Complex B { get; }
        public Complex C { get; }
        public Complex D { get; }

        public long Dimension => 2;
        public int NumQubits => 1;

        public Gate(Complex a, Complex b, Complex c, Complex d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public Qubit Transform(Qubit q) => new Qubit(q.AmpZero * A + q.AmpOne * B, q.AmpZero * C + q.AmpOne * D);
        public Gate Scale(Complex c) => new Gate(A * c, B * c, C * c, D * c);
        public IQuantumState Transform(IQuantumState input)
        {
            if (input.Dimension != 2)
            {
                throw new ArgumentException(nameof(input));
            }
            return Transform(new Qubit(input.GetAmplitude("0"), input.GetAmplitude("1")));
        }
        // this will only work if its a unitary transformation
        public Gate Inverse() =>
            new Gate(A.Conjugate(), C.Conjugate(), B.Conjugate(), D.Conjugate())
            .Scale(1 / (A * D - B * C).Magnitude); // need to take inverse of the scale incase were not unitary

        IUnitaryTransform IUnitaryTransform.Inverse() => Inverse();
    }

    public struct CGate : IUnitaryTransform
    {
        public Gate Gate { get; }

        public long Dimension => 4;
        public int NumQubits => 2;

        public CGate(Gate g)
        {
            Gate = g;
        }

        public MultiQubit Transform(Qubit control, Qubit target)
        {
            var init = new MultiQubit(control, target);
            return (MultiQubit)Transform(init);
        }

        public IQuantumState Transform(IQuantumState input)
        {
            if (input.Dimension != 4)
            {
                //throw new ArgumentException(nameof(input));
            }

            return new MultiQubit(new[]
            {
                input.GetAmplitude("00".ToBits()),
                input.GetAmplitude("01".ToBits()),
                input.GetAmplitude("10".ToBits()) * Gate.A + input.GetAmplitude("11".ToBits()) * Gate.B,
                input.GetAmplitude("10".ToBits()) * Gate.C + input.GetAmplitude("11".ToBits()) * Gate.D,
            });
        }

        // I have no idea if this is right
        public IUnitaryTransform Inverse() => new CGate(Gate.Inverse());
    }

    public static class Gates
    {
        public static readonly Gate I = new Gate(1, 0, 0, 1);

        // Pauli matrices
        public static readonly Gate X = new Gate(0, 1, 1, 0);
        public static readonly Gate Y = new Gate(0, -Complex.ImaginaryOne, Complex.ImaginaryOne, 0);
        public static readonly Gate Z = new Gate(1, 0, 0, -1);

        // Hadamard
        public static readonly Gate H = new Gate(1, 1, 1, -1).Scale(ComplexExt.OneOverRootTwo);
        // Phase gate
        public static readonly Gate S = new Gate(1, 0, 0, Complex.ImaginaryOne);

        // Pie/8 gate
        public static readonly Gate T = new Gate(1, 0, 0, Complex.Exp(Complex.ImaginaryOne * Math.PI / 8));

        // flips one amp and two amp
        // classical not gate
        public static readonly Gate Not = new Gate(0, 1, 1, 0);

        public static Gate Phase(int n) =>
            new Gate(1, 0, 0, Complex.Exp((Complex.ImaginaryOne * 2 * Math.PI) / Math.Pow(2, n)));
    }

    public class IdentityTransform : IUnitaryTransform
    {
        public long Dimension { get; }
        public int NumQubits { get; }

        public IdentityTransform(int numQubits)
        {
            NumQubits = numQubits;
            Dimension = (long)Math.Pow(2, NumQubits);
        }

        public IUnitaryTransform Inverse() => this;
        public IQuantumState Transform(IQuantumState input) => input;
    }

    public class MultiGate : IUnitaryTransform
    {       
        public long Dimension { get; }

        public int NumQubits => throw new NotImplementedException();

        private readonly Complex[,] elements;

        public MultiGate(Complex[,] elements)
        {
            if (elements.GetLongLength(0) != elements.GetLongLength(1))
            {
                throw new ArgumentException("Must be square matrix", nameof(elements));
            }

            if (!elements.GetLongLength(0).IsPowerTwo())
            {
                throw new ArgumentException("Must be power of 2");
            }

            this.elements = elements;
            Dimension = elements.GetLongLength(0);
        }

        public static MultiGate Identity(int dim)
        {
            long bigN = (long)Math.Pow(2, dim);
            var eles = new Complex[bigN, bigN];
            for(long i = 0; i < bigN; i++)
            {
                eles[i, i] = 1;
            }
            return new MultiGate(eles);
        }

        public IQuantumState Transform(IQuantumState input)
        {
            if (input.Dimension != Dimension)
            {
                throw new ArgumentException(nameof(input.Dimension));
            }

            throw new NotImplementedException();
        }

        public IUnitaryTransform Inverse()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Applies a transform to a specified set of quibit indexes on a quantum state
    /// </summary>
    public class PartialTransform : IUnitaryTransform
    {
        private readonly IUnitaryTransform transform;
        private readonly int[] applyToQubitIndexes;

        public long Dimension { get; }
        public int NumQubits { get; }

        public PartialTransform(long dimension, IUnitaryTransform transform, int[] applyToQubitIndexes)
        {
            if (transform.Dimension != (long)Math.Pow(2, applyToQubitIndexes.Length))
            {
                throw new ArgumentException();
            }
            if (!Dimension.IsPowerTwo())
            {
                throw new ArgumentException();
            }

            this.transform = transform;
            this.applyToQubitIndexes = applyToQubitIndexes;
            Dimension = dimension;
            NumQubits = (int)Math.Log(Dimension, 2);
        }

        public IQuantumState Transform(IQuantumState input)
        {
            if (transform.Dimension > input.Dimension)
            {
                throw new ArgumentException(nameof(transform.Dimension));
            }

            var newAmps = new Complex[Dimension];

            // foreach basis vector in the state
            for (long i = 0; i < Dimension; i++)
            {
                bool[] basis = new ComputationalBasis(i, NumQubits).GetLabels();
                Complex origAmp = input.GetAmplitude(basis); 
                // apply the subroutine to part of that basis
                IQuantumState res = transform.Transform(
                    new MultiQubit(
                        applyToQubitIndexes
                        .Select(index => basis[index] ? Qubit.ClassicOne : Qubit.ClassicZero)
                        .ToArray()));

                // redistribute the result to every possible basis
                for (long j = 0; j < transform.Dimension; j++)
                {
                    bool[] subRoutineBasis = new ComputationalBasis(j, transform.NumQubits).GetLabels();
                    Complex amp = res.GetAmplitude(subRoutineBasis);
                    for (int indexIndex = 0; indexIndex < transform.NumQubits; indexIndex++)
                    {
                        int qubitIndex = applyToQubitIndexes[indexIndex];
                        basis[qubitIndex] = subRoutineBasis[indexIndex];
                    }
                    long ampIndex = ComputationalBasis.FromLabels(basis).AmpIndex;
                    newAmps[ampIndex] += origAmp * amp;
                }
            }

            // weirder
            return new MultiQubit(newAmps);
        }

        public IUnitaryTransform Inverse()
            => new PartialTransform(Dimension, transform.Inverse(), applyToQubitIndexes);
    }

    public class CompositeTransform : IUnitaryTransform
    {
        private readonly ImmutableList<IUnitaryTransform> transforms;

        public long Dimension => transforms.Select(t => t.Dimension).Distinct().Single();
        public int NumQubits => transforms.Select(t => t.NumQubits).Distinct().Single();

        public CompositeTransform(params IUnitaryTransform[] transforms)
        {
            this.transforms = transforms.ToImmutableList();
        }

        public CompositeTransform(ImmutableList<IUnitaryTransform> transforms)
        {
            this.transforms = transforms;
        }

        public IQuantumState Transform(IQuantumState input)
            => transforms.Aggregate(input, (accum, next) => next.Transform(accum));

        public IUnitaryTransform Inverse()
            => new CompositeTransform(transforms.Reverse().Select(t => t.Inverse()).ToArray());

        public CompositeTransform Apply(IUnitaryTransform transform) 
            => new CompositeTransform(transforms.Add(transform));

        public CompositeTransform Apply(Gate g, int qubitIndex)
            => new CompositeTransform(transforms.Add(new PartialTransform(Dimension, g, new[] { qubitIndex })));

        public CompositeTransform ApplyControlled(Gate g, int controlIndex, int targetIndex)
            => new CompositeTransform(
                transforms.Add(new PartialTransform(Dimension, new CGate(g), new[] { controlIndex, targetIndex })));
    }

}
