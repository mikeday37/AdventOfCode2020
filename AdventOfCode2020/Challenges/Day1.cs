using AdventOfCodeScaffolding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2020.Challenges
{
	[Challenge(1, "Report Repair")]
	class Day1 : ChallengeBase
	{
		public override object Part1(string input)
		{
			const int target = 2020;

			var nums = input
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => int.Parse(x.Trim()))
				.OrderBy(x => x)
				.ToArray();

			int a = 0, b = nums.Length - 1;
			do
			{
				var sum = nums[a] + nums[b];
				if (sum == target)
					return nums[a] * nums[b];
				else if (sum < target)
					a++;
				else
					b--;
			}
			while (a < b);

			throw new Exception("No such pair exists.");
		}

		public override object Part2(string input)
		{
			const int target = 2020;

			var nums = input
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => int.Parse(x.Trim()))
				.OrderBy(x => x)
				.ToArray();

			int a = 0, b = 1, c = nums.Length - 1;
			do
			{
				var sum = nums[a] + nums[b] + nums[c];
				if (sum == target)
					return nums[a] * nums[b] * nums[c];
				else if (sum > target)
				{
					c--;
					a = 0;
					b = 1;
				}
				else
				{
					for (; ;)
					{
						b++;
						sum = nums[a] + nums[b] + nums[c];
						if (b == c || sum > target)
							break;
						else if (sum == target)
							return nums[a] * nums[b] * nums[c];
					}
					b = 1 + ++a;
				}
			}
			while (b < c);

			throw new Exception("No such triple exists.");
		}
	}
}
