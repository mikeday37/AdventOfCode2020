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
			var tester = new MessageTester(rules, base.AllowCancel);
			return messages.Count(x => tester.IsValid(x));
		}

		public override object Part2(string input)
		{
			ParseRulesAndMessages(input + "\n\n8: 42 | 42 8\n11: 42 31 | 42 11 31", out var rules, out var messages);
			var tester = new MessageTester(rules, base.AllowCancel);
			return messages.Count(x => tester.IsValid(x));
		}

		public class MessageTester
		{
			public MessageTester(IReadOnlyDictionary<int, Rule> rules, Action allowCancel = null)
			{
				this.allowCancel = allowCancel;
				rootLink = GrowInitialTree(rules);
				FindAllCycles();
				//DetermineBounds();
				LogTree("Tree Results:");
			}

			private void AllowCancel()
			{
				allowCancel?.Invoke();
			}

			private readonly Action allowCancel;
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

			/// <summary>
			/// Tells the node the current state of the match matchine, including the result of the immediately prior
			/// action requested by the same node, if any.
			/// </summary>
			private record MatchState
			{
				public Node Node {get; init;}
				public int EntryCursor {get; init;}
				public int Cursor {get; init;}
				public object NodeState {get; init;}
				public bool? PrevLinkWasMatch {get; init;}
				public bool ReturnedToBacktrack {get; init;}
			}

			/// <summary>
			/// Tells the match machine what to do or how to modify state.
			/// 
			/// Any nullable property set to not-null in this record is taken as an instruction for the match machine
			/// to take action and/or modify state.  Null means "don't do it" or "leave it as-is".
			/// </summary>
			private record MatchStepResult
			{
				public Link NextLink {get; init;}
				public int? NextCursor {get; init;}
				public bool? ReturnIsMatch {get; init;}
				public object NextNodeState {get; init;}
				public bool AllowBacktrack {get; init;}
			}

			private class MatchStackEntry
			{
				public int EntryCursor {get;set;}
				public Node Node {get;set;}
				public object NodeState {get;set;}

				public MatchStackEntry Clone() => new(){
					EntryCursor = this.EntryCursor,
					Node = this.Node,
					NodeState = (int?)this.NodeState // TODO: remove this unnecessary hedge
				};
			}

			// some very handy logging helpers
			static IDisposable C(string message) => ThreadLogger.Context(message);
			static void L(string message) => ThreadLogger.LogLine(message);

			/// <summary>
			/// Determine if the given message is valid according to the rules provided.
			/// </summary>
			public bool IsValid(string message)
			{
				FullMatchMachineState fullState = null;
				RunTracking tracking = new();

				using (C($"IsValid({message})"))
				for (; ;)
				{
					AllowCancel();

					if (IsValid(message, fullState, tracking))
					{
						L("RESULT = true.");
						return true;
					}
					else if (tracking.BacktrackQueue.Any())
					{
						L("BACKTRACKING...");
						fullState = tracking.BacktrackQueue.Dequeue();
					}
					else
					{
						L("RESULT = false.");
						return false;
					}
				}
			}

			private record FullMatchMachineState
			{
				public int Cursor {get; init;}
				public Stack<MatchStackEntry> Stack {get; init;}
				public MatchState State {get; init;}
				public int RunID {get; init;}
				public FullMatchMachineState() => Stack = new();
			}

			private class RunTracking
			{
				public Queue<FullMatchMachineState> BacktrackQueue {get;}
				public int NextRunID {get; set;}
				public RunTracking() => (BacktrackQueue, NextRunID) = (new(), 1);
			}

			/// <summary>
			/// Continue from the given full-match-machine-state to determine if the given message is valid according to the rules provided
			/// </summary>
			private bool IsValid(string message, FullMatchMachineState fullState, RunTracking tracking)
			{
				if (message == null)
					throw new ArgumentNullException(nameof(message));

				// init if not provided full state
				bool init = fullState == null;
				if (init)
					fullState = new(){RunID = tracking.NextRunID++};

				// sanity check the run/init tracking:
				// make sure "init" flag and run == 1 are consistent, and that if init, nextRunID == 2
				bool isRun1 = fullState.RunID == 1;
				if (init != isRun1)
					throw new Exception($"Unexpected behavior - init != isRun1.  init = {init}, isRun1 = {isRun1}, fullState.RunID = {fullState.RunID}, tracking.NextRunID = {tracking.NextRunID}");
				if (init && tracking.NextRunID != 2)
					throw new Exception($"Unexpected behavior - init is true but nextRunID != 2.  tracking.NextRunID = {tracking.NextRunID}");

				// we're going to be sharing a cursor along a "recursive" stack of node match logic "calls"
				int cursor = fullState.Cursor;
				Stack<MatchStackEntry> stack = fullState.Stack;
				MatchState state = fullState.State;
				MatchStepResult result;

				// define a method to push a new node onto the stack, to "call into" its match logic
				void Push(Node node)
				{
					var e = new MatchStackEntry{
						EntryCursor = cursor,
						Node = node,
						NodeState = node.Stateful ? node.NewInitialState() : null
					};
					stack.Push(e);
					state = new(){
						Node = node,
						EntryCursor = cursor,
						Cursor = cursor,
						NodeState = e.NodeState
					};
				}

				// define a method to pop a node's result from the stack, "returning the value" from that node's match logic
				void Pop()
				{
					stack.Pop();
					if (stack.Any())
					{
						var e = stack.Peek();
						state = new(){
							Node = e.Node,
							EntryCursor = e.EntryCursor,
							Cursor = cursor,
							NodeState = e.NodeState,
							PrevLinkWasMatch = result.ReturnIsMatch
						};
					}
					else
						state = null;
				}

				// define a method to enable backtracking from that point when appropriate
				void EnableBacktrackIfAppropriate()
				{
					// enable back tracking only if allowed by the result, indicated potentially required by the node, and deemed necessary
					var enableBacktrack = result.AllowBacktrack
						&& state.Node.MayRequireBacktracking
						// right now we deem it "necessary" only if the node returned from a cyclic link, and is an AlternativeNode
						// TODO: there are almost certainly more or less cases where its necessary - needs full analysis
						&& (state.Node.ParentLink?.Cyclic ?? false)
						&& state.Node is AlternativeNode;

					if (enableBacktrack)
					{
						// enable backtrack simply by copying the full match matchine state to enqueue onto the backtrackQueue,
						// with state modified to set the ReturnedToBacktrack flag
						var fms = new FullMatchMachineState{
							Cursor = cursor,
							Stack = new(stack.Reverse().Select(x => x.Clone())),
							State = state with {ReturnedToBacktrack = true},
							RunID = tracking.NextRunID++
						};
						tracking.BacktrackQueue.Enqueue(fms);
						IL($"SAVE BACKTRACK: RunID = {fms.RunID}");
					}
				}

				// if initializing, start by pushing ("calling into") into the rootLink's node (rule 0)
				if (init)
					Push(rootLink.Child);

				// log some useful info about this run
				L($"RunID = {fullState.RunID}");

				// helper method to log at indent, defaulting to indent by current stack size
				void IL(string message, int? depth = null) =>
					L($"{new string('\t', depth ?? stack.Count)}{message}");

				// log the current stack
				foreach (var e in stack.Reverse().WithIndex().SkipLast(1))
					IL($"{e.item.Node.ParentLink} | {e.item.Node} | EntryCursor = {e.item.EntryCursor}", e.index + 1);

				if (!init)
					IL("CONTINUING FROM BACKTRACK");

				// do the equivalent of recursion into Node logic, but via a stack-based match machine,
				// repeatedly taking steps into Node logic (.TakeStep()) at the top of the stack.  each step tells
				// the machine what to do next, always including either a Push() or a Pop() (never both from the same step).
				//
				// we'll have our final result when the stack is reduced to nothing after the final Pop().
				do
				{
					AllowCancel();

					// log the top of the stack
					var top = stack.Peek();
					IL($"{top.Node.ParentLink} | {top.Node} | EntryCursor = {top.EntryCursor} | cursor = {cursor}");

					// take a step on the current node
					result = state.Node.TakeStep(message, state);

					// enable backtracking to this point if needed
					EnableBacktrackIfAppropriate();

					// if the result indicates to alter the cursor, do so
					if (result.NextCursor.HasValue)
						cursor = result.NextCursor.Value;

					// if the result indicates to modify node-specific state, do so
					if (result.NextNodeState != null)
						top.NodeState = result.NextNodeState;

					// determine which "direction" we're going: 
					//
					//   "down" the stack (as in decreasing its size), returning from a sublink
					//    neither - not allowed
					//   "up" the stack (as in increasing its size), recursing into a sublink
					//
					// NOTE: "which way is up?" is a surprisingly unanswered question when it comes to stacks, without a lot more details.
					//       see: https://stackoverflow.com/questions/1677415/does-stack-grow-upward-or-downward
					//
					bool goingDown = result.ReturnIsMatch.HasValue;
					bool goingUp = result.NextLink != null;
					if (goingDown && goingUp)
						throw new Exception("MatchStepResult conflict - ReturnIsMatch and NextLink are both not null - set either, not both.");
					if ((!goingDown) && (!goingUp))
						throw new Exception("MatchStepResult invalid - both ReturnIsMatch and NextLink are null - either (not both) must be set.");

					// if going up, push the next link, otherwise pop the result down to the prev node
					if (goingUp)
						Push(result.NextLink.Child);
					else
					{
						IL($"{(result.ReturnIsMatch.Value ? "MATCH" : "FAIL")}");
						Pop();
					}

					// repeat as long as there's anything on the stack
				}
				while (stack.Any());

				// sanity check that result was set and it has a returned "IsMatch" value
				if (result == null)
					throw new NullReferenceException("result is null - this should not be possible.");
				if (!result.ReturnIsMatch.HasValue)
					throw new NullReferenceException("result provided but ReturnIsMatch is null - this should not be possible.");

				// it's truly a match only if the cursor is now just past the end of the message
				return result.ReturnIsMatch.Value && cursor == message.Length;
			}

			private abstract class Node
			{
				public Link ParentLink {get; set;}
				public List<Link> ChildLinks {get;}
				public Node() => ChildLinks = new();
				public virtual bool Stateful => false;
				public virtual object NewInitialState() => throw new NotImplementedException($"NewInitialState() not implemented on Type {this.GetType().Name} (Stateful = {Stateful}).");
				public virtual bool MayRequireBacktracking => false;

				public override string ToString()
				{
					return this.GetType().Name.Replace("Node", "");
				}

				public abstract MatchStepResult TakeStep(string message, MatchState state);
			}

			private abstract class BranchNode<TNodeState> : Node
				where TNodeState : struct
			{
				public override bool Stateful => true;
				public override object NewInitialState() => default(TNodeState);

				public override MatchStepResult TakeStep(string message, MatchState state) =>
					TakeStep(message, state, (TNodeState)state.NodeState);

				protected abstract MatchStepResult TakeStep(string message, MatchState state, TNodeState nodeState);
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

			private class SequenceNode : BranchNode<int>
			{
				protected override MatchStepResult TakeStep(string message, MatchState state, int indexInSequence)
				{
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
						indexInSequence++;
					}

					// if we have now checked all elements in the sequence without failure, then we're a match
					if (indexInSequence >= base.ChildLinks.Count)
						return new MatchStepResult{ReturnIsMatch = true};
					else
						// otherwise we have to descend into the sublink for the next element
						return new MatchStepResult{
							NextLink = base.ChildLinks[indexInSequence],
							NextNodeState = indexInSequence,
						};
				}
			}

			private class AlternativeNode : BranchNode<int>
			{
				public override bool MayRequireBacktracking => true;

				protected override MatchStepResult TakeStep(string message, MatchState state, int indexInAlternatives)
				{
					// if we're returning to backtrack, we must advance to the next alternative
					if (state.ReturnedToBacktrack)
						indexInAlternatives++;
					else 
					{
						// otherwise, if we previously checked a sublink
						if (state.PrevLinkWasMatch.HasValue)
						{
							// and it did match, then we succeed immediately, but also have to allow potential backtracking
							if (state.PrevLinkWasMatch.Value)
								return new MatchStepResult{
									ReturnIsMatch = true,
									AllowBacktrack = true
								};

							// otherwise, advance to the next alternative
							indexInAlternatives++;
						}
					}

					// if we have now checked all elements in the sequence without success, then we're not a match,
					// and must reset the cursor
					if (indexInAlternatives >= base.ChildLinks.Count)
						return new MatchStepResult{
							ReturnIsMatch = false,
							NextCursor = state.EntryCursor
						};
					else
						// otherwise we have to reset the cursor and descend into the sublink for the next alternative
						return new MatchStepResult{
							NextCursor = state.EntryCursor,
							NextLink = base.ChildLinks[indexInAlternatives],
							NextNodeState = indexInAlternatives
						};
				}
			}
		}
	}
}
