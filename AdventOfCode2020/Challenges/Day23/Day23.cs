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
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AdventOfCode2020.Challenges.Day23
{
	[Challenge(23, "Crab Cups")]
	public class Day23Challenge : ChallengeBase
	{
		public class Cup
		{
			public int Label {get; init;}
			public Cup Next {get; set;}
		}

		public Cup ParseCupsAndReturnTop(string input)
		{
			Cup first = null, prev = null, cup = null;
			foreach (var c in input.Trim())
			{
				cup = new Cup{Label = int.Parse(new string(c, 1))};
				if (prev == null)
					first = cup;
				else
					prev.Next = cup;
				prev = cup;
			}
			cup.Next = first;
			return first;
		}

		public IEnumerable<Cup> FollowCupsOnce(Cup start)
		{
			if (start == null)
				yield break;
			var cur = start;
			do
			{
				yield return cur;
				cur = cur.Next;
			}
			while (cur != null && cur != start);
		}

		public override object Part1(string input)
		{
			var (cup1, lowest, highest) = PlayCrabCups(ParseCupsAndReturnTop(input), 100, true);

			// calculate result string
			var result = new string(FollowCupsOnce(cup1.Next).Take(highest - lowest).Select(x => $"{x.Label}"[0]).ToArray());
			Logger.LogLine($"Result: {result}");

			return result;
		}

		public override object Part2(string input)
		{
			var top = ParseCupsAndReturnTop(input);
			Logger.LogLine("Extending...");
			ExtendToOneMillionCups(top);
			var a = FollowCupsOnce(PlayCrabCups(top, 10000000, false).cup1.Next).Take(2).ToArray();
			return (long)a[0].Label * (long)a[1].Label;
		}

		private void ExtendToOneMillionCups(Cup top)
		{
			var n = FollowCupsOnce(top).Select(x => x.Label).Max();
			Cup prev = FollowCupsOnce(top).Last(), cur = null;
			while (++n <= 1000000)
			{
				cur = new Cup{Label = n};
				prev.Next = cur;
				prev = cur;
			}
			cur.Next = top;
		}

		public (Cup cup1, int lowest, int highest) PlayCrabCups(Cup top, int moves, bool verboseLogging)
		{
			Logger.LogLine("Tracking...");
			Dictionary<int, (bool available, Cup cup)> track = FollowCupsOnce(top)
				.OrderBy(x => x.Label).ToDictionary(x => x.Label, x => (true, x));
			var (lowest, highest) = (track.Keys.Min(), track.Keys.Max());
			Logger.LogLine("Moving...");

			// do X moves
			var cur = top;
			foreach (var move in Enumerable.Range(1, moves))
			{
				// log move state
				if (verboseLogging)
				{
					Logger.LogLine($"\n-- move {move} --");
					Logger.LogLine($"cups: {string.Join(" ", FollowCupsOnce(top).Select(x => x == cur ? $"({x.Label})" : $"{x.Label}"))}");
				}
				else if (0 == move % 10000)
					Logger.LogLine($"on move {move}...");

				// pick up 3 cups after cur
				var (a, b) = (cur.Next, FollowCupsOnce(cur.Next).Take(3).Last());
				cur.Next = b.Next;
				b.Next = null;
				if (verboseLogging) Logger.LogLine($"pick up: {string.Join(", ", FollowCupsOnce(a).Select(x => x.Label))}");

				// mark picks as unavailable
				foreach (var picked in FollowCupsOnce(a))
					track[picked.Label] = (false, picked);

				// determine dest
				int dest = cur.Label;
				do
				{
					if (--dest < lowest)
						dest = highest;
				}
				while (!track[dest].available);
				if (verboseLogging) Logger.LogLine($"destination: {dest}");

				// mark picks as available again
				foreach (var picked in FollowCupsOnce(a))
					track[picked.Label] = (true, picked);

				// insert picks next of dest
				var destCup = track[dest].cup;
				b.Next = destCup.Next;
				destCup.Next = a;

				// pick next cup
				cur = cur.Next;
			}

			// log final positions
			if (verboseLogging)
			{
				Logger.LogLine($"\n-- final --");
				Logger.LogLine($"cups: {string.Join(" ", FollowCupsOnce(top).Select(x => x == cur ? $"({x.Label})" : $"{x.Label}"))}");
			}

			// return cup 1, lowest, and highest
			return (track[1].cup, lowest, highest);
		}
	}
}
