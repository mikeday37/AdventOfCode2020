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

namespace AdventOfCode2020.Challenges.Day19
{
	[Challenge(19, "Monster Messages")]
	class Day19Challenge : ChallengeBase
	{
		/*
		 * <rule> =
		 *   #: <req>
		 * 
		 * <req> = one of
		 *   <char>
		 *   <list>
		 *   <alt>
		 *	 
		 * <list> =
		 *    space-delimited list of non-negative integers
		 * 
		 * <alt> =
		 *	  <list> | <list>
		 * 
		 * <char> = one of
		 *    "a"
		 *    "b"
		 *    
		 */

		public enum RuleType {Character, Sequence, AlternativeSequences}

		public record Rule
		{
			public int Index {get;}
			public RuleType Type {get;}
			public char Character {get;}
			public int[] ByIndexSequence {get;}
			public int[][] ByIndexAlternativeSequences {get;}

			public Rule[] Sequence {get; set;}
			public Rule[][] AlternativeSequences {get; set;}

			public Rule(string line)
			{
				var a = line.Split(": ");
				Index = int.Parse(a[0]);

				switch (a[1])
				{
					case var s when s[0] == '\"':
						Type = RuleType.Character;
						Character = s[1];
						break;

					case var s when s.Contains('|'):
						Type = RuleType.AlternativeSequences;
						ByIndexAlternativeSequences = s
							.Split(" | ")
							.Select(x => x
								.Split(' ')
								.Select(y => int.Parse(y))
								.ToArray()
							)
							.ToArray();
						break;

					default:
						Type = RuleType.Sequence;
						ByIndexSequence = a[1]
							.Split(' ')
							.Select(y => int.Parse(y))
							.ToArray();
						break;
				}
			}
		}

		static void ParseRulesAndMessages(string input, out Dictionary<int, Rule> rules, out List<string> messages)
		{
			var r = rules = new(); // meh, can't use the out param directly in the later lambdas (error CS1628)
			messages = new();

			// parse the input into rules (by index) and messages
			foreach (var line in input.ToLines())
				if (char.IsDigit(line[0]))
				{
					var rule = new Rule(line);
					rules[rule.Index] = rule;
				}
				else
					messages.Add(line);

			// stitch the rules together directly, so we no longer have to refer to the dictionary to walk the tree
			foreach (var rule in rules.Values)
				switch (rule.Type)
				{
					case RuleType.Sequence:
						rule.Sequence = rule.ByIndexSequence.Select(x => r[x]).ToArray();
						break;

					case RuleType.AlternativeSequences:
						rule.AlternativeSequences = rule.ByIndexAlternativeSequences
							.Select(x => x.Select(y => r[y]).ToArray())
							.ToArray();
						break;
				}
		}

		static string GetRegexPattern(Rule r)
		{
			switch (r.Type)
			{
				case RuleType.Character: return $"{r.Character}";
				case RuleType.Sequence: return string.Concat(r.Sequence.Select(x => GetRegexPattern(x)));
				case RuleType.AlternativeSequences:
					StringBuilder sb = new();
					sb.Append('(');
					bool first = true;
					foreach (var alt in r.AlternativeSequences)
					{
						if (!first) sb.Append('|');
						first = false;
						foreach (var part in alt)
							sb.Append(GetRegexPattern(part));
					}
					sb.Append(')');
					return sb.ToString();

				default: throw new Exception($"Bad rule type: {r.Type}");
			};
		}

		public override object Part1(string input)
		{
			ParseRulesAndMessages(input, out var rules, out var messages);
			var pattern = $"^{GetRegexPattern(rules[0])}$";
			var regex = new Regex(pattern, RegexOptions.Compiled);
			return messages.Count(x => regex.IsMatch(x));
		}

		public override object Part2(string input)
		{
			ParseRulesAndMessages(input, out var rules, out var messages);

			// sanity check rule 0
			if (!(
					rules[0].Type == RuleType.Sequence
					&& Enumerable.SequenceEqual(rules[0].ByIndexSequence, new[]{8, 11})
				))
				throw new Exception("ERIC!!!!!!!!!"); // -- kirk, wrath of khan

			var r42 = GetRegexPattern(rules[42]);
			var r31 = GetRegexPattern(rules[31]);

			var pattern = $"^(?<a>{r42})+(?<b>{r31})+$";

			var regex = new Regex(pattern, RegexOptions.Compiled);
			bool IsMatch(string s)
			{
				var match = regex.Match(s);
				var a = match.Groups["a"].Captures.Count;
				var b = match.Groups["b"].Captures.Count;
				return match.Success && a - b >= 1;
			}
				
			return messages.Count(x => IsMatch(x));
		}
	}
}
