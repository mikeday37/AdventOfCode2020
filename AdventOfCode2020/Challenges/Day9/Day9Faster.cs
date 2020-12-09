using AdventOfCodeScaffolding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges.Day9
{
	[Challenge(9, "Encoding Error - Faster")]
	class Day9FasterChallenge : ChallengeBase
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
			var nums = /**/input
				.Split('\n')
				/*/
				"100 200 301 402 500 600".Split(' ')
				/**/
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => long.Parse(x))
				.ToArray();

			var range = FindTargetRange(target, nums);
			if (range != null)
			{
				var slice = nums[range.Value];
				return slice.Min() + slice.Max();
			}

			return -1;
		}

		static Range? FindTargetRange(long target, long[] nums)
		{
			//Debug.WriteLine($"\n\n\n=============================\nnums.Length = {nums.Length}\n");

			
			for (int outerStart = 0, outerEnd = nums.Length - 1; outerStart < outerEnd; outerStart++, outerEnd--)
			{
				//Debug.WriteLine($"\n[{outerStart:D4} to {outerEnd:D4}]");

				long sum = 0;

				void add(int index)
				{
					//Debug.WriteLine($"\t+[{index}]");
					sum += nums[index];
				}

				void sub(int index)
				{
					//Debug.WriteLine($"\t-[{index}]");
					sum -= nums[index];
				}

				Range range(int a, int b)
				{
					var r = a..(b + 1);
					//Debug.WriteLine($"\t\t{r}");
					return r;
				}

				add(outerStart);

				for (int i = outerStart + 1; i <= outerEnd; i++)
				{
					add(i);
					//Debug.WriteLine("\t\t?");
					if (sum == target)
						return range(outerStart, i);
				}
				
				for (int i = outerStart; i < outerEnd - 1; i++)
				{
					sub(i);
					//Debug.WriteLine("\t\t?");
					if (sum == target)
						return range(i + 1, outerEnd);
				}
			}

			return null;
		}
	}
}
