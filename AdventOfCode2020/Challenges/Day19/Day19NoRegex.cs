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
			private class MatchStackEntry
			{
				public int EntryCursor {get;set;}
				public Node Node {get;set;}
				public int State {get;set;}
			}

			public bool IsValid(string message)
			{
				/*int cursor = 0;
				Stack<MatchStackEntry> stack = new();

				MatchStackEntry Push(Node node)
				{
					var entry = new MatchStackEntry{EntryCursor = cursor, Node = node};
					stack.Push(entry);
					return entry;
				}

				var top = Push(rootLink.Child);
				var bottom = top;

				for (; ;)
				{
					
				}*/

				return false; // todo
			}

			public MessageTester(IReadOnlyDictionary<int, Rule> rules)
			{
				rootLink = GrowInitialTree(rules);
				FindAllCycles();
				//DetermineBounds();
				LogTree("Tree Results:");
			}

			private readonly Link rootLink;

			/// <summary>
			/// Takes the dictionary of rules and implements them as a "tree" of Node objects connected by Link objects,
			/// setting the PreExisting flag on all but the "first" Link to any given rule.  Returns the rootLink of the tree.
			/// </summary>
			/// <remarks>
			/// The resulting "tree" is only a strict tree in the sense that you can treat it as a tree if you don't follow links where
			/// PreExisting == true.  If you ignore that flag, the result is actually a directed graph, which may contain any number
			/// of cycles (infinite loops).  The logic and types used by this method safely handle all such loops in a reliable way,
			/// and sets us up to efficiently identify all cycles (if any) for later analysis.
			/// </remarks>
			private static Link GrowInitialTree(IReadOnlyDictionary<int, Rule> rules)
			{
				// the root is always for rule 0
				Link rootLink = new(){Rule = rules[0]};

				// we'll need to keep track of what node implements what rule, in order to set the PreExisting flags correctly.
				Dictionary<int, Node> ruleNodes = new();
				
				// start growing the tree from its root
				HashSet<Link> unresolvedLinks = new();
				unresolvedLinks.Add(rootLink);

				// keep growing until there are no more links left unresolved
				using (ThreadLogger.Context("Growing Initial Tree..."))
					while (unresolvedLinks.Any())
						foreach (var link in unresolvedLinks.ToList()) // .ToList() because we change the hashset we're iterating over
							GrowInitialTree(link);

				// return the root now that it's fully grown
				return rootLink;

				// helper method to grow the tree one unresolved link at a time:
				//
				// take the given unresolvedLink (AL), remove it from the set, and resolving it by
				// creating the Node(s) and Link(s) (if any) responsible for directly implementing its Rule,
				// and add to the set a new unresolvedLink for each sub-rule any of those new nodes must reference.
				void GrowInitialTree(Link AL)
				{
					// remove from the set, since it will be resolved when we're done.
					unresolvedLinks.Remove(AL);

					// see if a node already exists for the rule of this unresolved link
					bool preExisting = ruleNodes.TryGetValue(AL.Rule.Index, out var A);
					if (preExisting)
					{
						// if so, just mark the link as pointing to a "PreExisting" node
						AL.PreExisting = true;

						// and set it's child to the node we already found.
						AL.Child = A;

						// note: could mark A's "additional parent links" at this point if desired
					}
					else
					{
						// otherwise, we must actually implement the rule, depending on its type
						switch (AL.Rule.Type)
						{
							// A Character rule is directly implemented by a single node (A) with no further references.
							//
							// AL -> A
							//
							case RuleType.Character: 
								A = new CharacterNode{Content = AL.Rule.Character};
								break;

							// A Sequence rule is implemented by a node for the sequence itself (A), having as children
							// a new (initially unresolved) Link (BL) for each rule it references.
							//
							// AL -> A ->> BL
							//
							case RuleType.Sequence: 
								A = new SequenceNode();
								foreach (var b in AL.Rule.Sequence)
								{
									var BL = new Link{Parent = A, Rule = rules[b]};
									A.ChildLinks.Add(BL);
									unresolvedLinks.Add(BL);
								}
								break;
							
							// An AlternativeSequence is implemented as a sub-tree exactly 
							// like a Sequence of Sequences, except that the root Node (A) of this sub-tree
							// is actually an AlternativeNode instead of a SequenceNode.
							//
							// AL -> A ->> BL -> B ->> CL
							//
							case RuleType.AlternativeSequences:
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
										unresolvedLinks.Add(CL);
									}
								}
								break;
						}

						// connect the link to the new root of the subtree we just created
						AL.Child = A;
						A.ParentLink = AL;

						// record the node as implemented for the rule
						ruleNodes[AL.Rule.Index] = A;
					}
				}
			}

			/// <summary>
			/// Efficiently finds and marks all links that directly cause a cycle (infinite loop), and all links that
			/// reference them directly or not, which are thus implemented by cycles.  This is important for reliably
			/// determining which nodes must support backtracking.
			/// </summary>
			private void FindAllCycles()
			{
				using (ThreadLogger.Context("Finding Cycles..."))
				{
					HashSet<Node> stack = new();
					HashSet<Link> potential = new(), cyclic = new();
					int directCycles = 0;

					void RecursivelySearchForCycles(Link link)
					{
						var node = link.Child;
						potential.Add(link);
						stack.Add(node);

						foreach (var childLink in node.ChildLinks)
						{
							if (stack.Contains(childLink.Child))
							{
								directCycles++;
								childLink.Cyclic = childLink.DirectlyCyclic = true;
								ThreadLogger.LogLine($"Directly Cyclic link found for Rule {childLink.ClosestContainingRule.Index}.");
								foreach (var p in potential)
									cyclic.Add(p);
								potential.Clear();
							}
							else
								RecursivelySearchForCycles(childLink);
						}

						stack.Remove(node);
						potential.Remove(link);
					}

					using (ThreadLogger.Context("Finding Direct Cycles..."))
						RecursivelySearchForCycles(rootLink);

					using (ThreadLogger.Context("Marking Indirect Cycles..."))
						foreach (var l in cyclic)
						{
							l.Cyclic = true;
							ThreadLogger.LogLine($"Indirectly Cyclic link found for Rule {l.ClosestContainingRule.Index}.");
						}

					ThreadLogger.LogLine($"Cycle Count: Direct = {directCycles}, Indirect = {cyclic.Count}, Total = {cyclic.Count + directCycles}");
				}
			}

			/// <summary>
			/// Recursively logs the tree, safely avoiding any infinite loops.
			/// The "PreExisting" flag is used to avoid repeatedly logging the
			/// details of rules that have already been logged.
			/// 
			/// The result is that every rule that is actually referenced is
			/// fully logged.
			/// </summary>
			private void LogTree(string message)
			{
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

				using (ThreadLogger.Context(message))
					LogTree(rootLink);
			}

			private class Link
			{
				public Node Parent {get; set;}
				public Node Child {get; set;}
				public Rule Rule {get; set;}
				public bool PreExisting {get; set;}
				public bool Cyclic {get;set;}
				public bool DirectlyCyclic {get;set;}
				public Link ClosestContainingRuleLink {get{
					var l = this;
					while (l.Rule == null)
					{
						l = l.Parent?.ParentLink;
						if (l == null) throw new NullReferenceException("Null reference while attempting to find ClosestContainingRuleLink.");
					}
					return l;
				}}
				public Rule ClosestContainingRule => ClosestContainingRuleLink.Rule;

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


			private record MatchState
			{
				public Node Node {get; init;}
				public int EntryCursor {get; init;}
				public int Cursor {get; init;}
				public object NodeState {get; init;}
				public bool? PrevLinkWasMatch {get; init;}
			}

			/// <summary>
			/// Anything not-null in this record is taken as an instruction for the match machine
			/// to take action and/or modify state.  Null means "don't do it" or "leave it as-is".
			/// </summary>
			private record MatchStepResult
			{
				public Link NextLink {get; init;}
				public int? NextCursor {get; init;}
				public bool? ReturnIsMatch {get; init;}
				public object NextNodeState {get; init;}
			}

			private abstract class Node
			{
				public Link ParentLink {get; set;}
				public List<Link> ChildLinks {get;}
				public Node() => ChildLinks = new();
				public virtual bool Stateful => false;
				public virtual bool MayRequireBacktracking => false;

				public override string ToString()
				{
					return this.GetType().Name.Replace("Node", "");
				}

				public abstract MatchStepResult TakeStep(string message, MatchState state);
			}

			private abstract class BranchNode<TNodeState> : Node
				where TNodeState : class, new()
			{
				public override bool Stateful => true;
				public virtual TNodeState NewState() => new TNodeState();

				protected static TNodeState GetNodeState(MatchState state) => (TNodeState)state.NodeState;
			}

			private class CharacterNode : Node
			{
				public char Content {get; set;}

				public override string ToString()
				{
					return $"{base.ToString()} = {Content}";
				}

				public override MatchStepResult TakeStep(string message, MatchState state)
				{
					bool isMatch;
					if (state.Cursor >= message.Length)
						isMatch = false;
					else
						isMatch = message[state.Cursor] == Content;

					return new MatchStepResult{
						NextCursor = state.Cursor + (isMatch ? 1 : 0),
						ReturnIsMatch = isMatch
					};
				}
			}

			private class SequenceNode : BranchNode<SequenceNode.NodeState>
			{
				public class NodeState
				{
					public int NextElement = 0;
				}

				public override MatchStepResult TakeStep(string message, MatchState state)
				{
					var nodeState = GetNodeState(state);

					// if we previously checked a sublink
					if (state.PrevLinkWasMatch.HasValue)
					{
						// and it didn't match, then we fail immediately, resetting the cursor
						if (!state.PrevLinkWasMatch.Value)
							return new MatchStepResult{
								ReturnIsMatch = false,
								NextCursor = state.EntryCursor
							};

						// otherwise we advance to next element
						nodeState.NextElement++;
					}

					// if we have now checked all elements in the sequence without failure, then we're a match
					if (nodeState.NextElement >= base.ChildLinks.Count)
						return new MatchStepResult{ReturnIsMatch = true};
					else
						// otherwise we have to descend into the sublink for the next element
						return new MatchStepResult{
							NextLink = base.ChildLinks[nodeState.NextElement],
							NextNodeState = nodeState,
						};
				}
			}

			private class AlternativeNode : BranchNode<AlternativeNode.NodeState>
			{
				public class NodeState
				{
					public int NextAlternative = 0;
				}

				public override bool MayRequireBacktracking => true;

				public override MatchStepResult TakeStep(string message, MatchState state)
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
