using AdventOfCodeScaffolding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges
{
	[Challenge(6, "Custom Customs")]
	class Day6 : ChallengeBase
	{
		public override object Part1(string input)
		{
			var lines = input
				.Split('\n')
				.Select(x => x.Trim())
				.ToList();

			var build = "";
			int count = 0;

			foreach (var line in lines)
			{
				if (string.IsNullOrWhiteSpace(line))
				{
					if (string.IsNullOrWhiteSpace(build))
						continue;

					count += CountDistinctLetters(build);

					build = "";
				}
				else
					build += line + " ";
			}


			count += CountDistinctLetters(build);

			return count;
		}

		private int CountDistinctLetters(string build)
		{
			return build
				.ToCharArray()
				.Where(x => x != ' ')
				.Distinct()
				.Count();
		}

		public override object Part2(string input)
		{
			var lines = input
				.Split('\n')
				.Select(x => x.Trim())
				.ToList();

			var build = "";
			int count = 0;

			foreach (var line in lines)
			{
				if (string.IsNullOrWhiteSpace(line))
				{
					if (string.IsNullOrWhiteSpace(build))
						continue;

					count += CountCommonLetters(build);

					build = "";
				}
				else
					build += line + " ";
			}

			if (!string.IsNullOrWhiteSpace(build))
				count += CountCommonLetters(build);

			return count;
		}

		private int CountCommonLetters(string build)
		{
			return build
				.Split(' ')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => x.ToCharArray().AsEnumerable())
				.Aggregate((a,b)=>a.Intersect(b))
				.Count();
		}
	}
}
