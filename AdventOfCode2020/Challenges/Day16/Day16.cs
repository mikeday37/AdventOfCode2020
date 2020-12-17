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

namespace AdventOfCode2020.Challenges.Day16
{
	[Challenge(16, "Ticket Translation")]
	class Day16Challenge : ChallengeBase
	{
		public record InclusiveRange
		{
			public int Min {get; init;}
			public int Max {get; init;}

			public InclusiveRange(int min, int max) => (Min, Max) = (min, max);

			public static InclusiveRange Parse(string s)
			{
				var p = s.Split('-');
				return new InclusiveRange(int.Parse(p[0]), int.Parse(p[1]));
			}

			public bool IsValid(int i) => Min <= i  && i <= Max;
		}

		public record Rule 
		{
			public string Text {get; init;}
			public InclusiveRange[] Ranges {get; init;}

			public Rule(string text, IEnumerable<InclusiveRange> ranges) => (Text, Ranges) = (text, ranges.ToArray());

			public static Rule Parse(string s)
			{
				var a = s.Split(": ");
				var b = a[1].Split(" or ");
				return new Rule(a[0], b.Select(x => InclusiveRange.Parse(x)));
			}

			public bool IsValid(int i) => Ranges.Any(x => x.IsValid(i));
		}

		public record Ticket
		{
			public int[] Values {get; init;}

			public Ticket(IEnumerable<int> values) => Values = values.ToArray();

			public static Ticket Parse(string s) => new Ticket(s.Split(',').Select(x => int.Parse(x)));
		}

		public record PuzzleInput
		{
			public Rule[] Rules {get; init;}
			public Ticket MyTicket {get; init;}
			public Ticket[] NearbyTickets {get; init;}

			public PuzzleInput(IEnumerable<Rule> rules, Ticket myTicket, IEnumerable<Ticket> nearbyTickets)
				=> (Rules, MyTicket, NearbyTickets) = (rules.ToArray(), myTicket, nearbyTickets.ToArray());

			public static PuzzleInput Parse(string input)
			{
				var lines = input.ToLines().ToList();
				var linesArray = lines.ToArray();
				var a = lines.IndexOf("your ticket:");
				var b = lines.IndexOf("nearby tickets:");
				return new PuzzleInput(
					linesArray[..a].Select(x => Rule.Parse(x)),
					Ticket.Parse(linesArray[a + 1]),
					linesArray[(b + 1)..].Select(x => Ticket.Parse(x))
				);
			}
		}

		public override object Part1(string rawInput)
		{
			var input = PuzzleInput.Parse(rawInput);

			return input
				.NearbyTickets
				.SelectMany(x => x.Values)
				.Where(x => input
					.Rules
					.SelectMany(y => y.Ranges)
					.All(y => !y.IsValid(x))
				)
				.Sum();
		}

		public override object Part2(string rawInput)
		{
			var input = PuzzleInput.Parse(rawInput);

			var goodNearbyTickets = input
				.NearbyTickets
				.Where(x => x
					.Values
					.All(y => input // all of the values in the ticket...
						.Rules
						.Any(z => z.IsValid(y)) // must be valid by at least one rule
					)
				)
				.ToList();

			var fieldCount = goodNearbyTickets.First().Values.Length;

			var fieldValues = Enumerable
				.Range(0, fieldCount)
				.Select(i => new {
					fieldIndex = i,
					goodTicketValues = goodNearbyTickets
						.Select(t => t.Values[i])
						.ToArray()
				})
				.ToList();

			var applicableRulesPerField = fieldValues
				.ToDictionary(
					x => x.fieldIndex,
					x => input
						.Rules
						.Where(rule => x
							.goodTicketValues
							.All(v => rule.IsValid(v))
						)
						.ToHashSet()
				);

			Dictionary<int, Rule> ruleMap = new();
			foreach (var i in Enumerable.Range(0, fieldCount))
			{
				var rule = ruleMap[i] = applicableRulesPerField[i].Single();
				for (var r = i + 1; r < fieldCount; r++)
					applicableRulesPerField[r].Remove(rule);
			}

			var fieldIndicesOfInterest = ruleMap
				.Where(x => x.Value.Text.StartsWith("departure"))
				.Select(x => x.Key)
				.ToList();

			return fieldIndicesOfInterest
				.Select(x => input.MyTicket.Values[x])
				.Aggregate((a,b)=>a*b);
		}
	}
}
