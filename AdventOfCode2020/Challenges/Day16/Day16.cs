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

			// determine all the "good" tickets by removing all that have any value which can't match any rule
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


			// for each field, get all the good ticket values for that field
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

			// get dict of field index -> all rules for which all values at that index are valid
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

			// there must be a single field which only has one applicable rule,
			// which we can then remove from availability and put in the rulemap,
			// and repeat until there's nothing more available.
			Dictionary<int, Rule> ruleMap = new();
			while (applicableRulesPerField.Any())
			{
				var isolated = applicableRulesPerField
					.First(x => x.Value.Count == 1);

				applicableRulesPerField.Remove(isolated.Key);

				var rule = isolated.Value.Single();

				foreach (var a in applicableRulesPerField)
					a.Value.Remove(rule);

				ruleMap[isolated.Key] = rule;
			}

			// then just look up the indices of the field values of interest
			var fieldIndicesOfInterest = ruleMap
				.Where(x => x.Value.Text.StartsWith("departure"))
				.Select(x => x.Key)
				.ToList();

			// and get the product of the corresponding values on my ticket
			return fieldIndicesOfInterest
				.Select(x => (long)input.MyTicket.Values[x])
				.Aggregate((a,b)=>a*b);
		}
	}
}
