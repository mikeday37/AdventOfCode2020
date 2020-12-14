using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * This file is essentially a scratch pad for quick experiments.
 * Don't take it as representative of any notion of proper ways to do anything.  :)
 * 
 */

namespace AdHocConsole
{
	class Program
	{
		static (long, long)[] ParseFleet(string descriptor)
		{
			return descriptor
				.Trim()
				.Split(',')
				.Select(x => x.Trim())
				.Select((x,i) => (x,i))
				.Where(x => x.x != "x")
				.Select(x => (long.Parse(x.x), (long)x.i))
				.ToArray();
		}

		static long CorrectedMod(long n, long divisor)
		{
			var result = n % divisor;
			if (result < 0) result += divisor;
			return result;
		}

		static long SolveFleet(string descriptor)
		{
			var fleet = ParseFleet(descriptor);
			return Enumerable.Range(0, 2000000000)
				.FirstOrDefault(x =>
					fleet.All(b => b.Item2 == CorrectedMod(-x, b.Item1)) // math-correct version of:  b.Item2 == -x (mod b.Item1)
				);
		}
					//fleet.All(b => b.Item2 == (b.Item1 - (x % b.Item1)) % b.Item1)
					// ^^ original version at time of AoC win

		static void Main(string[] args)
		{
			void a(string d) {
				Console.WriteLine($"\n{d} => {SolveFleet(d)}");
				Console.WriteLine("solve " + string.Join(" and ", ParseFleet(d).Select(x => $"{x.Item2} = -x mod {x.Item1}")));
			}

			Console.WriteLine($"{(-13) % 7} -- {CorrectedMod(-13, 7)}");

			a("17,x,13,19");
			a("67,7,59,61");
			a("67,x,7,59,61");
			a("67,7,x,59,61");
			//a("1789,37,47,1889");

			Console.ReadKey();
		}

		void SeatIdTest()
		{
			Console.WriteLine(ParseSeatID("BFFFBBFRRR"));
			Console.WriteLine(ParseSeatID("FFFBBBFRRR"));
			Console.WriteLine(ParseSeatID("BBFFBBFRLL"));

			Console.ReadKey();
		}

		static int ParseSeatID(string bsp)
		{
			return Convert.ToInt32(
				bsp
					.Replace('F','0')
					.Replace('B','1')
					.Replace('L','0')
					.Replace('R','1')
				,
				2);
		}
	}
}
