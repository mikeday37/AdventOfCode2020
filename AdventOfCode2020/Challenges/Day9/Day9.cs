using AdventOfCodeScaffolding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges.Day9
{
	[Challenge(9, "Encoding Error")]
	class Day9Challenge : ChallengeBase
	{
		public override object Part1(string input)
		{
			var nums = input
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => long.Parse(x))
				.ToArray();
			
			foreach (var at in Enumerable.Range(25, nums.Length - 25))
			{
				AllowCancel();

				var preamble = nums[(at - 25)..at];
				var cur = nums[at];
				if (!CanSumToFromDistinct(cur, preamble))
					return cur;
			}

			return -1;
		}

		private bool CanSumToFromDistinct(long cur, long[] preamble)
		{
			// brute force for now
			foreach (var x in preamble)
				foreach (var y in preamble)
					if (x != y && x + y == cur)
						return true;
			return false;
		}

		public override object Part2(string input)
		{
			long target = (long)Part1(input); // heh.
			var nums = input
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => long.Parse(x))
				.ToArray();

			// say goodbye to performance...
			foreach (var start in Enumerable.Range(0, nums.Length))
			{
				using (Logger.Context($"start = {start}"))
				foreach (var end in Enumerable.Range(start + 1, nums.Length - 1 - start))
				{
					if (end % 100 == 0)
					{
						Logger.LogLine($"end = {end}");
						Thread.Sleep(250);
					}

					AllowCancel();

					var suspect = nums[start..end];
					if (suspect.Sum() == target)
						return suspect.Min() + suspect.Max();
				}
			}

			return -1;
		}
	}
}
