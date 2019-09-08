using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HelloQuantum
{
    public static class BitExt
    {
        public static string ToBitString(this IEnumerable<bool> bits) => string.Join("", bits.Select(bit => bit ? '1' : '0'));
        public static bool[] ToBits(this string bits) => bits.Select(c => c == '1').ToArray();
    }

    public static class ComplexExt
    {
        public const double Precision = 0.00000001;

        public static readonly Complex OneOverRootTwo = new Complex(1 / Math.Sqrt(2), 0);

        public static IEnumerable<Complex> Normalize(this IEnumerable<Complex> nums)
        {
            double sqrtNorm = Math.Sqrt(nums.TwoNorm());
            return nums.Select(n => n / sqrtNorm);
        }

        public static bool IsNormalized(this IEnumerable<Complex> nums)
            => (nums.TwoNorm() - 1.0) < 0.00000001; // arb threshold

        public static double TwoNorm(this IEnumerable<Complex> nums)
        {
            double total = 0;
            foreach (Complex num in nums)
            {
                total += num.Magnitude * num.Magnitude;
            }
            return total;
        }

        public static Complex Conjugate(this Complex c) => new Complex(c.Real, -c.Imaginary);
    }

    public static class LongExt
    {
        public static bool IsPowerTwo(this long num) => (num & num) == num;

        public static IEnumerable<long> Range(long start, long range)
        {
            for (long i = start; i < start + range; i++)
            {
                yield return i;
            }
        }

        public static long FromBits(bool[] bits)
        {
            long total = 0;
            int k = 0;
            foreach (var item in bits)
            {
                k++;
                if (!item) continue;
                total += (long)Math.Pow(2, bits.Length - k);
            }
            return total;
        }

        public static IEnumerable<bool> ToBits(this long n)
        {
            if (n < 2)
            {
                return new[] { n == 1 };
            }

            var divisor = n / 2;
            var remainder = n % 2;

            return ToBits(divisor).Concat(new[] { remainder == 1 });
        }

        public static bool[] ToBitsPad(this long n, long padLength)
        {
            bool[] bits = n.ToBits().ToArray();
            // need to pad with zeros to get the correct number of labels
            return new bool[padLength - bits.Length].Concat(bits).ToArray();
        }

        /// <summary>
        /// The minimum number of bits required to represent this number
        /// </summary>
        public static int BitsCeiling(this long n) => (int)Math.Ceiling(Math.Log(n, 2));
    }
}
