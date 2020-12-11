using AdventOfCodeScaffolding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges.Day10
{
	[Challenge(10, "Adapter Array")]
	class Day10Challenge : ChallengeBase
	{
		/*
		 * outlet : outputs 0 jolts
		 * 
		 * adapters : output X jolts (puzzle input line)
		 *          : can take input of X-1, X-2, or X-3
		 *          
		 * device : input = 3 + Max(X)
		 * 
		 */

		static long[] ParseJoltages(string input)
		{
			return input.ToLines()
				.Select(x => long.Parse(x))
				.OrderBy(x => x)
				.ToArray();
		}

		public override object Part1(string input)
		{
			var joltages = ParseJoltages(input);

			long d1 = 0,
				d3 = 1; // because of device, not in list, always 3 higher than max

			long prev = 0;
			foreach (var j in joltages)
			{
				if (j - prev == 1) d1++; else d3++;
				prev = j;
			}
				
			return d1 * d3;
		}

		public class AdapterInfo
		{
			public long
				joltage = 0,
				minusPrior = 0;

			public bool
				removable = false;

			public override string ToString() => $"{joltage:D5} {minusPrior} {(removable?'*':' ')}";
		}

		static AdapterInfo[] AnalyzeJoltages(long[] joltages)
		{
			var augmented = new long[1]{0}
				.Concat(joltages)
				.Append(3+joltages[^1])
				.ToArray();

			var diffed = augmented
				.Skip(1)
				.Select((x, i) => x - augmented[i])
				.ToArray();

			var analyzed = augmented
				.Select((x, i) => new AdapterInfo{
					joltage = x,
					minusPrior = x == 0 ? 0 : diffed[i - 1]
				})
				.ToArray();

			foreach (var (x,i) in analyzed.Select((x,i)=>(x,i)).ToArray()[1..^1].Reverse().Skip(1))
				x.removable = x.minusPrior == 1 && analyzed[i + 1].minusPrior == 1;

			return analyzed;
		}

		public override object Part2(string input)
		{
			var joltages = ParseJoltages(input);

			var analyzed = AnalyzeJoltages(joltages);
			//var printableAnalysis = string.Join('\n', analyzed.Select(x => x.ToString()));

			Dictionary<int, int> contiguousRemovableLengths = new();
			int? contiguouslyRemovable = null;
			void chop() {
				if (contiguouslyRemovable.HasValue)
				{
					var r = contiguouslyRemovable.Value;
					contiguousRemovableLengths[r] = 1 + contiguousRemovableLengths.GetValueOrDefault(r, 0);
					contiguouslyRemovable = null;
				}
			};
			foreach (var a in analyzed[1..^1])
			{
				if (a.removable)
					contiguouslyRemovable = 1 + (contiguouslyRemovable ?? 0);
				else
					chop();
			}
			chop();

			var variationCount = contiguousRemovableLengths
				.SelectMany(x => Enumerable.Repeat(
						(long) (x.Key switch {
							1 => 2,
							2 => 4,
							3 => 7, // would be 8, except that would violate the rules - must leave one
							// and luckily there are no larger contiguousRemovableLengths
							_ => throw new Exception($"Unexpected contiguousRemovableLength: {x.Key}")
						}),
						x.Value
					)
				)
				.Aggregate(1L, (a, b) => a * b);
				
			return variationCount;
		}
	}
}
