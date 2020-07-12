using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
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

        /// <summary>
        /// The number of primitive quantum gates it takes to represent this transformation.
        /// </summary>
        int NumGates { get; }

        /// <summary>
        /// Some transforms have shortcuts when repeatadly applying them.
        /// This allows overriding the default which is just repeated application.
        /// </summary>
        IUnitaryTransform Pow(long exponent);
        
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
        public int NumGates => 1;

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

        public IUnitaryTransform Pow(long exponent)
        {
            IUnitaryTransform wtf = this;
            return new CompositeTransform(LongExt.Range(0, exponent).Select(exp => wtf).ToArray());
        }           
    }

    public struct CTransform : IUnitaryTransform
    {
        public IUnitaryTransform InnerTransform { get; }

        public long Dimension => 2 * InnerTransform.Dimension;
        public int NumQubits => 1 + InnerTransform.NumQubits;
        public int NumGates => InnerTransform.NumGates; // i think this is considered a basic gate

        public CTransform(IUnitaryTransform innerTransform)
        {
            InnerTransform = innerTransform;
        }

        public IQuantumState Transform(IQuantumState input)
        {
            if (input.Dimension != 2 * InnerTransform.Dimension)
            {
                throw new ArgumentException(nameof(input));
            }           

            var firstHalf = new Complex[Dimension / 2];
            var secHalf = new Complex[Dimension / 2];
            for (long i = 0; i < Dimension; i++)
            {
                // first half of vector is just copied over as an identity
                if (i < Dimension / 2)
                {
                    firstHalf[i] = input.GetAmplitude(new ComputationalBasis(i, NumQubits)); 
                }
                else
                {
                    secHalf[i - (Dimension / 2)] = input.GetAmplitude(new ComputationalBasis(i, NumQubits));
                }
            }

            var transformedSecHalf = InnerTransform.Transform(new MultiQubit(secHalf));

            return new MultiQubit(firstHalf.Concat(transformedSecHalf).ToArray());
        }

        public IUnitaryTransform Inverse() => new CTransform(InnerTransform.Inverse());

        public IUnitaryTransform Pow(long exp)
            => new CTransform(InnerTransform.Pow(exp));
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

        // it takes nothing to do nothing
        public int NumGates => 0;

        public IdentityTransform(int numQubits)
        {
            NumQubits = numQubits;
            Dimension = (long)Math.Pow(2, NumQubits);
        }

        public IUnitaryTransform Inverse() => this;
        public IQuantumState Transform(IQuantumState input) => input;

        public IUnitaryTransform Pow(long exponent) => this;
    }

    public class MultiGate : IUnitaryTransform
    {
        public long Dimension { get; }
        public int NumQubits { get; }
        // this is usually a 'simulation' of multiple smaller gates
        public int NumGates { get; }

        private readonly Matrix<Complex> matrix;

        // optional way to pow func
        private readonly Func<long, IUnitaryTransform> powFunc;

        public MultiGate(Complex[,] elements, int numGates, Func<long, IUnitaryTransform> powFunc = null)
        {
            if (elements.GetLongLength(0) != elements.GetLongLength(1))
            {
                throw new ArgumentException("Must be square matrix", nameof(elements));
            }

            if (!elements.GetLongLength(0).IsPowerTwo())
            {
                throw new ArgumentException("Must be power of 2");
            }

            Dimension = elements.GetLongLength(0);
            NumQubits = (int)Math.Log(Dimension, 2);
            matrix = DenseMatrix.OfArray(elements);
            NumGates = numGates;
            this.powFunc = powFunc;
        }

        public IQuantumState Transform(IQuantumState input)
        {
            if (input.Dimension != Dimension)
            {
                throw new ArgumentException(nameof(input.Dimension));
            }

            DenseVector inVec = DenseVector.OfArray(input.ToArray());
            var res = matrix * inVec;
            return new MultiQubit(res.ToArray());
        }

        public IUnitaryTransform Inverse() => new MultiGate(matrix.Inverse().ToArray(), NumGates);

        public IUnitaryTransform Pow(long exp)
        {
            if (exp == 1)
            {
                return this;
            }
            if (powFunc == null)
            {
                return ((IUnitaryTransform)this).Pow(exp);
            }
            return powFunc(exp);
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

        public int NumGates => transform.NumGates;

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

        public IUnitaryTransform Pow(long exp)
            => new PartialTransform(Dimension, transform.Pow(exp), applyToQubitIndexes);
    }

    public class CompositeTransform : IUnitaryTransform
    {
        private readonly ImmutableList<IUnitaryTransform> transforms;

        public long Dimension { get; }
        public int NumQubits { get; }

        public int NumGates { get; }

        public CompositeTransform(params IUnitaryTransform[] transforms)
        {
            this.transforms = transforms.ToImmutableList();
            Dimension = transforms.Select(t => t.Dimension).Distinct().Single();
            NumQubits = transforms.Select(t => t.NumQubits).Distinct().Single();
            NumGates = transforms.Sum(t => t.NumGates);
        }

        public CompositeTransform(ImmutableList<IUnitaryTransform> transforms)
        {
            this.transforms = transforms;
            Dimension = transforms.Select(t => t.Dimension).Distinct().Single();
            NumQubits = transforms.Select(t => t.NumQubits).Distinct().Single();
            NumGates = transforms.Sum(t => t.NumGates);
        }

        public IQuantumState Transform(IQuantumState input)
            => transforms.Aggregate(input, (accum, next) =>
            {
                return next.Transform(accum);
            });

        public IUnitaryTransform Inverse()
            => new CompositeTransform(transforms.Reverse().Select(t => t.Inverse()).ToArray());

        public CompositeTransform Apply(IUnitaryTransform transform, long exp = 1) 
            => new CompositeTransform(transforms.Add(transform.Pow(exp)));

        public CompositeTransform Apply(Gate g, int qubitIndex, long exp = 1)
        {
            IUnitaryTransform newTrans = new PartialTransform(Dimension, g, new[] { qubitIndex });
            return new CompositeTransform(transforms.Add(newTrans.Pow(exp)));
        }

        public CompositeTransform Apply(IUnitaryTransform g, int[] qubitIndexes, long exp = 1)
        {
            IUnitaryTransform newTrans = new PartialTransform(Dimension, g, qubitIndexes);
            return new CompositeTransform(transforms.Add(newTrans.Pow(exp)));
        }

        public CompositeTransform ApplyControlled(IUnitaryTransform g, int controlIndex, int targetIndex, long exp = 1)
            => ApplyControlled(g, controlIndex, new[] { targetIndex }, exp);

        public CompositeTransform ApplyControlled(IUnitaryTransform g, int controlIndex, int[] targetIndexes, long exp = 1)
        {
            IUnitaryTransform newTrans = new PartialTransform(
                    Dimension,
                    new CTransform(g),
                    new[] { controlIndex }.Concat(targetIndexes).ToArray());
            return new CompositeTransform(transforms.Add(newTrans.Pow(exp)));
        }

        public IUnitaryTransform Pow(long exponent) => 
           new CompositeTransform(LongExt.Range(0, exponent).Select(exp => this).ToArray());
    }

}
