using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.ThirdParty.RosettaCode
{
    /// <summary>
    /// Provides a means to solve simultaneous modulo equations via the Chinese Remainder Theorem,
    /// from Rosetta Code.  See remarks for attribution.
    /// </summary>
    /// <remarks>
    /// This class and all its content, except the comments, were copied on December 13, 2020, from:
    /// <a href="https://rosettacode.org/wiki/Chinese_remainder_theorem#C.23">the C# implementation of the Chinese Remainder Theorem page at the Rosetta Code website.</a>
    /// That content is available under the <a href="https://www.gnu.org/licenses/fdl-1.2.html">GNU Free Documentation License 1.2</a>
    /// which also exists in the root of this solution under the filename: "fdl-1.2.txt"
    /// 
    /// The following modifications were made by Mike Day:  all occurances of the word "int" were replaced with "long",
    /// and the first occurance of the number "1" was replaced with "1L".
    /// </remarks>
    public static class ChineseRemainderTheorem
    {
        public static long Solve(long[] n, long[] a)
        {
            long prod = n.Aggregate(1L, (i, j) => i * j);
            long p;
            long sm = 0;
            for (long i = 0; i < n.Length; i++)
            {
                p = prod / n[i];
                sm += a[i] * ModularMultiplicativeInverse(p, n[i]) * p;
            }
            return sm % prod;
        }
 
        private static long ModularMultiplicativeInverse(long a, long mod)
        {
            long b = a % mod;
            for (long x = 1; x < mod; x++)
            {
                if ((b * x) % mod == 1)
                {
                    return x;
                }
            }
            return 1;
        }
    }
 }
