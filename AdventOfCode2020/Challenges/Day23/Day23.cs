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
			// parse the labels into cups and setup a tracking dictionary, marking all as initially available
			Cup top, cur;
			top = cur = ParseCupsAndReturnTop(input);
			Dictionary<int, (bool available, Cup cup)> track = FollowCupsOnce(top)
				.OrderBy(x => x.Label).ToDictionary(x => x.Label, x => (true, x));
			var (lowest, highest) = (track.Keys.Min(), track.Keys.Max());

			// do X moves
			const int moves = 100;
			foreach (var move in Enumerable.Range(1, moves))
			{
				// log move state
				Logger.LogLine($"\n-- move {move} --");
				Logger.LogLine($"cups: {string.Join(" ", FollowCupsOnce(top).Select(x => x == cur ? $"({x.Label})" : $"{x.Label}"))}");

				// pick up 3 cups after cur
				var (a, b) = (cur.Next, FollowCupsOnce(cur.Next).Take(3).Last());
				cur.Next = b.Next;
				b.Next = null;
				Logger.LogLine($"pick up: {string.Join(", ", FollowCupsOnce(a).Select(x => x.Label))}");

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
				Logger.LogLine($"destination: {dest}");

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
			Logger.LogLine($"\n-- final --");
			Logger.LogLine($"cups: {string.Join(" ", FollowCupsOnce(top).Select(x => x == cur ? $"({x.Label})" : $"{x.Label}"))}");

			// calculate result string
			var result = new string(FollowCupsOnce(track[1].cup.Next).Take(highest - lowest).Select(x => $"{x.Label}"[0]).ToArray());
			Logger.LogLine($"Result: {result}");

			return result;
		}

		public override object Part2(string input)
		{
			return -1;
		}
	}
}
