using AdventOfCodeScaffolding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges
{
	[Challenge(5, "Binary Boarding")]
	class Day5 : ChallengeBase
	{
		public override object Part1(string input)
		{
			return input
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x =>
					Convert.ToInt32(
						x
							.Replace('F','0')
							.Replace('B','1')
							.Replace('L','0')
							.Replace('R','1')
						,2
					)
				)
				.Max();
		}

		public override object Part2(string input)
		{
			var seats = input
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x =>
					Convert.ToInt32(
						x
							.Replace('F','0')
							.Replace('B','1')
							.Replace('L','0')
							.Replace('R','1')
						,
						2)
				)
				.ToHashSet();

			return Enumerable
				.Range(1, seats.Max() - 2)
				.Single(x =>
					seats.Contains(x - 1)
					&& ! seats.Contains(x)
					&& seats.Contains(x + 1)
				);
		}
	}
}
