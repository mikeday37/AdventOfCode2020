using AdventOfCodeScaffolding;
using AdventOfCode2020.ThirdParty.RosettaCode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges.Day13
{
	[Challenge(13, "Shuttle Search")]
	class Day13Challenge : ChallengeBase
	{
		public override object Part1(string input)
		{
			var a = input.ToLines().ToArray();
			var earliest = long.Parse(a[0]);
			var inService = a[1].Split(',').Where(x => x.Trim() != "x").Select(x => long.Parse(x)).OrderBy(x => x).ToArray();
			var pick = inService.Select(x => new {id = x, delay = x - earliest % x}).ToArray().OrderBy(x => x.delay).First();

			return pick.id * pick.delay;
		}

		public override object Part2(string input)
		{
			var descriptor = input.ToLines().Skip(1).Single();
			var fleet = ParseFleet(descriptor);

			// This outputs a query compatible with WolframAlpha, and actually works on several of the examples.
			// Sadly WolframAlpha does not accept it for the last example nor the full puzzle input.
			// see: https://www.wolframalpha.com/
			var simultaneousModuloEquation = string.Join(" and ", fleet.Select(x => $"{x.Item2} = x mod {x.Item1}"));
			Debug.WriteLine(simultaneousModuloEquation);

			var n = fleet.Select(x => x.Item1).ToArray();
			var a = fleet.Select(x => x.Item1 - x.Item2).ToArray();
			var answer = ChineseRemainderTheorem.Solve(n, a);

			return answer;
		}

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
	}
}
