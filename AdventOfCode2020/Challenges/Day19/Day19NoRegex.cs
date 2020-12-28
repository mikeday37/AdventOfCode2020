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
	[Challenge(19, "Monster Messages - No Regex")]
	class Day19ChallengeNoRegex : ChallengeBase
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
			public int[] Sequence {get;}
			public int[][] AlternativeSequences {get;}

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
						AlternativeSequences = s
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
						Sequence = a[1]
							.Split(' ')
							.Select(y => int.Parse(y))
							.ToArray();
						break;
				}
			}

			public static Rule Parse(string line) => new Rule(line);
		}

		static void ParseRulesAndMessages(string input, out Dictionary<int, Rule> rules, out List<string> messages)
		{
			rules = new();
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
		}

		public override object Part1(string input)
		{
			ParseRulesAndMessages(input, out var rules, out var messages);
			var tester = new MessageTester(rules);
			return messages.Count(x => tester.IsValid(x));
		}

		public override object Part2(string input)
		{
			ParseRulesAndMessages(input + "\n\n8: 42 | 42 8\n11: 42 31 | 42 11 31", out var rules, out var messages);
			var tester = new MessageTester(rules);
			return messages.Count(x => tester.IsValid(x));
		}

		public class MessageTester
		{
			public bool IsValid(string message)
			{
				return false; // todo
			}

			public MessageTester(IReadOnlyDictionary<int, Rule> rules)
			{
				rootLink = new(){Rule = rules[0]};
				unresolvedInitialLeafLinks.Add(rootLink);

				using (ThreadLogger.Context("Growing Initial Tree..."))
					while (unresolvedInitialLeafLinks.Any())
						foreach (var link in unresolvedInitialLeafLinks.ToList())
							GrowInitialTree(link, rules);

				using (ThreadLogger.Context("Finding Cycles..."))
				{
					HashSet<Link> stack = new(), potential = new(), cyclic = new();
					int directCycles = 0;

					void DFS(Link link)
					{
						if (stack.Contains(link))
						{
							link.Cyclic = true;
							link.DirectlyCyclic = true;
							directCycles++;
							ThreadLogger.LogLine($"Directly Cyclic link found for Rule {link.ClosestContainingRule.Index}.");
							foreach (var s in potential.ToList())
							{
								cyclic.Add(s);
								potential.Remove(s);
							}
						}
						else if (link.Child != null)
						{
							stack.Add(link);
							potential.Add(link);

							foreach (var childLink in link.Child.ChildLinks)
								DFS(childLink);

							potential.Remove(link);
							stack.Remove(link);
						}
					}

					using (ThreadLogger.Context("Finding Direct Cycles..."))
						DFS(rootLink);

					using (ThreadLogger.Context("Marking Indirect Cycles..."))
						foreach (var l in cyclic)
						{
							l.Cyclic = true;
							ThreadLogger.LogLine($"Indirectly Cyclic link found for Rule {l.ClosestContainingRule.Index}.");
						}

					ThreadLogger.LogLine($"Cycle Count: Direct = {directCycles}, Indirect = {cyclic.Count - directCycles}, Total = {cyclic.Count}");
				}

				static void LogTree(Link link)
				{
					using (ThreadLogger.Context(link.ToString()))
						if (link.Child == null)
							ThreadLogger.LogLine("<to-grow>");
						else if (!link.PreExisting)
							using (ThreadLogger.Context(link.Child.ToString()))
								foreach (var childLink in link.Child.ChildLinks)
									LogTree(childLink);
				}

				using (ThreadLogger.Context("Tree Results:"))
					LogTree(rootLink);
			}

			private readonly Link rootLink;
			private readonly Dictionary<int, Node> ruleNodes = new();
			private readonly HashSet<Link> unresolvedInitialLeafLinks = new();

			private void GrowInitialTree(Link AL, IReadOnlyDictionary<int, Rule> rules)
			{
				unresolvedInitialLeafLinks.Remove(AL);
				bool preExisting = ruleNodes.TryGetValue(AL.Rule.Index, out var A);
				if (preExisting)
				{
					AL.PreExisting = true;
					AL.Child = A;
					// note: could mark A's "additional parent links" at this point if desired
				}
				else
				{
					switch (AL.Rule.Type)
					{
						case RuleType.Character: // AL -> A
							A = new StringNode{Content = new string(AL.Rule.Character, 1)};
							break;

						case RuleType.Sequence: // AL -> A ->> BL
							A = new SequenceNode();
							foreach (var b in AL.Rule.Sequence)
							{
								var BL = new Link{Parent = A, Rule = rules[b]};
								A.ChildLinks.Add(BL);
								unresolvedInitialLeafLinks.Add(BL);
							}
							break;
							
						case RuleType.AlternativeSequences: // AL -> A ->> BL -> B ->> CL
							A = new AlternativeNode();
							foreach (var b in AL.Rule.AlternativeSequences)
							{
								var B = new SequenceNode();
								var BL = new Link{Parent = A, Child = B};
								A.ChildLinks.Add(BL);
								B.ParentLink = BL;
								foreach (var c in b)
								{
									var CL = new Link{Parent = B, Rule = rules[c]};
									B.ChildLinks.Add(CL);
									unresolvedInitialLeafLinks.Add(CL);
								}
							}
							break;
					}
					AL.Child = A;
					A.ParentLink = AL;
					ruleNodes[AL.Rule.Index] = A;
				}
			}

			private class Link
			{
				public Node Parent {get; set;}
				public Node Child {get; set;}
				public Rule Rule {get; set;}
				public bool PreExisting {get; set;}
				public bool Cyclic {get;set;}
				public bool DirectlyCyclic {get;set;}
				public Rule ClosestContainingRule {get{
					var l = this;
					while (l.Rule == null)
					{
						l = l.Parent?.ParentLink;
						if (l == null) throw new NullReferenceException("Null reference while attempting to find ClosestContainingRule.");
					}
					return l.Rule;
				}}

				public override string ToString()
				{
					var sb = new StringBuilder();

					if (Rule == null)
						sb.Append("(no rule)");
					else
						sb.Append($"Rule {Rule.Index,3}");

					if (PreExisting)
						sb.Append(" - Pre-Existing");

					if (DirectlyCyclic)
						sb.Append(" - Directly Cyclic");
					else if (Cyclic)
						sb.Append(" - Cyclic");

					return sb.ToString();
				}
			}

			private abstract class Node
			{
				public Link ParentLink {get; set;}
				public List<Link> ChildLinks {get;}
				public Node() => ChildLinks = new();
				public virtual bool Stateful => false;

				public override string ToString()
				{
					return this.GetType().Name.Replace("Node", "");
				}
			}

			private abstract class BranchNode<TState> : Node
			{
				public override bool Stateful => true;
			}

			private class StringNode : Node
			{
				public string Content {get; set;}

				public override string ToString()
				{
					return $"{base.ToString()} = {Content}";
				}
			}

			private class SequenceNode : BranchNode<SequenceNode.State>
			{
				public class State
				{
				}
			}

			private class AlternativeNode : BranchNode<AlternativeNode.State>
			{
				public class State
				{
				}
			}
		}
	}
}
