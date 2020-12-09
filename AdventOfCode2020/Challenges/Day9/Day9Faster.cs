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
		static long[] ParseNums(string input)
		{
			return input
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => long.Parse(x))
				.ToArray();
		}

		static bool CanSumToFromDistinct(long cur, long[] preamble)
		{
			// brute force for now
			foreach (var x in preamble)
				foreach (var y in preamble)
					if (x != y && x + y == cur)
						return true;
			return false;
		}

		static long FindTargetValue(long[] nums)
		{
			foreach (var at in Enumerable.Range(25, nums.Length - 25))
			{
				var preamble = nums[(at - 25)..at];
				var cur = nums[at];
				if (!CanSumToFromDistinct(cur, preamble))
					return cur;
			}

			throw new Exception("No such value found.");
		}

		static Range FindTargetRange(long target, long[] nums)
		{
			// consider successively shorter slices, narrowing at both ends simultaneously
			for (int outerStart = 0, outerEnd = nums.Length - 1; outerStart < outerEnd; outerStart++, outerEnd--)
			{
				static Range inclusiveRange(int a, int b) => a..(b+1); // this is just a little helper for greater clarity where it's used

				// within the slice, start from left and grow right, considering as we go
				long sum = nums[outerStart];
				for (int i = outerStart + 1; i <= outerEnd; i++)
				{
					sum += nums[i];
					if (sum == target)
						return inclusiveRange(outerStart, i);
				}
				
				// then shrink to right
				for (int i = outerStart; i < outerEnd - 1; i++)
				{
					sum -= nums[i];
					if (sum == target)
						return inclusiveRange(i + 1, outerEnd);
				}
			}

			throw new Exception("No such range found.");
		}

		public override object Part1(string input)
		{
			var nums = ParseNums(input);
			return FindTargetValue(nums);
		}

		public override object Part2(string input)
		{
			var nums = ParseNums(input);
			var target = FindTargetValue(nums);
			var range = FindTargetRange(target, nums);
			var slice = nums[range];
			return slice.Min() + slice.Max();
		}
	}
}
