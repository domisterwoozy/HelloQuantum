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
    }
}
