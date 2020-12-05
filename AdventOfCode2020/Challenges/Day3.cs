using AdventOfCodeScaffolding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2020.Challenges
{
	[Challenge(3, "Toboggan Trajectory")]
	class Day3 : ChallengeBase
	{
		public override object Part1(string input)
		{
			var lines = input
				.Split('\n')
				.Select(x => x.Trim())
				.Where(x => !string.IsNullOrWhiteSpace(x));

			int index = 0, count = 0, width = 0;

			foreach (var line in lines)
			{
				if (width == 0)
				{
					width = line.Length;
					continue;
				}

				index += 3;
				if (line[index % width] == '#')
					count++;
			}

			return count;
		}

		public override object Part2(string input)
		{
			var lines = input
				.Split('\n')
				.Select(x => x.Trim())
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.ToList();
			var lines2 = lines
				.Where((x, i) => 0 == i % 2);

			return
				CountTrees(1, lines) *
				CountTrees(3, lines) *
				CountTrees(5, lines) *
				CountTrees(7, lines) *
				CountTrees(1, lines2);
		}

		long CountTrees(int across, IEnumerable<string> lines)
		{
			int index = 0, count = 0, width = 0;

			foreach (var line in lines)
			{
				if (width == 0)
				{
					width = line.Length;
					continue;
				}

				index += across;
				if (line[index % width] == '#')
					count++;
			}

			return count;
		}
	}
}
