using AdventOfCodeScaffolding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges
{
	[Challenge(7, "Handy Haversacks")]
	class Day7 : ChallengeBase
	{
		public override object Part1(string input)
		{
			var rules = input
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => ParseRule(x))
				.ToDictionary(x => x.Key, x => x.Value);

			var validColors = new HashSet<string>();
			validColors.Add("shiny gold");
			int lastcount = 0;
			do
			{
				lastcount = validColors.Count;

				foreach (var rule in rules)
					if (rule.Value.Keys.Intersect(validColors).Any())
						validColors.Add(rule.Key);
			}
			while (validColors.Count > lastcount);

			return validColors.Count - 1;
		}

		KeyValuePair<string, Dictionary<string, int>> ParseRule(string english)
		{
			var a = english.Split(" bags contain ");
			var nested = new Dictionary<string, int>();
			var rule = new KeyValuePair<string, Dictionary<string, int>>(a[0], nested);

			if (a[1] != "no other bags.")
				foreach (var type in a[1].Split(", "))
				{
					var b = type.Split(' ');
					var count = int.Parse(b[0]);
					var color = b[1] + " " + b[2];
					nested[color] = count;
				}

			return rule;
		}

		public override object Part2(string input)
		{
			var rules = input
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => ParseRule(x))
				.ToDictionary(x => x.Key, x => x.Value);

			return BagsRequired("shiny gold", rules);
		}

		int BagsRequired(string color, Dictionary<string, Dictionary<string, int>> rules)
		{
			return rules[color]
				.Sum(x =>
					x.Value * (1 + BagsRequired(x.Key, rules))
				);
		}
	}
}
