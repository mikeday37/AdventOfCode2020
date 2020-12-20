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

namespace AdventOfCode2020.Challenges.Day18
{
	[Challenge(18, "Operation Order")]
	class Day18Challenge : ChallengeBase
	{
		public enum TokenType {None = 0, Add, Mult, LeftParen, RightParen, Value};

		public record Token
		{
			public TokenType Type {get; init;}
			public long Value {get; init;}

			public static Token NewValue(char c) => new Token{Type = TokenType.Value, Value = long.Parse(new string(c, 1))};
			public static Token NewSymbol(char c) => new Token{Type = c switch {
				'+' => TokenType.Add,
				'*' => TokenType.Mult,
				'(' => TokenType.LeftParen,
				')' => TokenType.RightParen,
				_ => throw new Exception($"Invalid token: {c}")
			}};

			public override string ToString() => $"[{Type}{(Type == TokenType.Value ? $"={Value}" : "")}]";
		}

		static IEnumerable<Token> Tokenize(string line)
		{
			/* 
			 * assumptions based on review of input and examples:
			 * 
			 * 1. all values are single digit.
			 * 
			 * 2. parenthesis tokens only exist in consecutive groups of 1 or more of the same type of parenthesis,
			 *	  always with one leading or trailing digit.  the placement of the digit is determined by the type of parenthesis,
			 *	  and there is no whitespace between any characters in the group.
			 *	  
			 * 3. all other consecutive token pairs are separated by a single space.
			 * 
			 * 4. as a consequence of all the above, if a space-split part has length > 1,
			 *    it is a value either prefixed or suffixed with one or more parentheses of one type.
			 * 
			 * 5. all expressions are valid.
			 * 
			 */

			foreach (var part in line.Split(' '))
				switch (part)
				{
					// #))))
					case var p when p.Length > 1 && char.IsDigit(p[0]):
						yield return Token.NewValue(p[0]);
						foreach (var c in p[1..])
							yield return Token.NewSymbol(c);
						break;

					// ((((#
					case var p when p.Length > 1 && char.IsDigit(p[^1]):
						foreach (var c in p[..^1])
							yield return Token.NewSymbol(c);
						yield return Token.NewValue(p[^1]);
						break;

					// #
					case var p when char.IsDigit(p[0]):
						yield return Token.NewValue(p[0]);
						break;

					// other
					default:
						yield return Token.NewSymbol(part[0]);
						break;
				}
		}

		private class Block
		{
			public Block Parent {get; set;}
			public List<object> Parts {get;}

			public void Add(Token t) => Parts.Add(t);

			public Block Nest()
			{
				var child = new Block();
				child.Parent = this;
				Parts.Add(child);
				return child;
			}

			public Block() => Parts = new();
		}

		static Block Blockify(IEnumerable<Token> tokens)
		{
			var root = new Block();
			var cur = root;

			foreach (var token in tokens)
				switch (token.Type)
				{
					case TokenType.LeftParen:   cur = cur.Nest(); break;
					case TokenType.RightParen:  cur = cur.Parent; break;
					default:                    cur.Add(token);   break;
				}

			return root;
		}

		static long Evaluate(object part)
		{
			switch (part)
			{
				case Token t when t.Type == TokenType.Value:
					return t.Value;
					
				case Block b:
					long result = Evaluate(b.Parts[0]);
					for (int i = 1; i < b.Parts.Count; i += 2)
					{
						var op = (Token)b.Parts[i];
						var rightValue = Evaluate(b.Parts[i+1]);
						switch (op.Type)
						{
							case TokenType.Add: result += rightValue; break;
							case TokenType.Mult: result *= rightValue; break;
							default: throw new Exception($"Unexpected op: {op}");
						}
					}
					return result;

				default:
					throw new Exception($"Unexpected direct part evaluation: {part}");
			}
		}

		public override object Part1(string input)
		{
			return input
				.ToLines()
				.Select(x => {
					var tokens = Tokenize(x);
					var root = Blockify(tokens);
					var result = Evaluate(root);
					return result;
				})
				.Sum();
		}

		static void ApplyAdvancedPrecedence(Block root)
		{
			// do it to all blocks in the tree
			for (int i = 0; i < root.Parts.Count; i += 2)
				if (root.Parts[i] is Block subBlock)
					ApplyAdvancedPrecedence(subBlock);

			// now check all operators in this block, unless the block is reduced to just three elements (then it's no longer necessary)
			var count = root.Parts.Count;
			for (int i = 1; i < count && count > 3; i += 2)
				if (root.Parts[i] is Token t && t.Type == TokenType.Add)
				{
					/*
					 * fold the high-precedence operator and its immediate neighbors into a new block replacing the operator.
					 * 
					 * like this:
					 * 
					 *   i          i            i 
					 * L O R  ->  L C O R  ->  C
					 *                         |
					 *                       L O R
					 * 
					 * heh...  this would've been a lot easier if I just used nodes with left/right links intead of a List<T>
					 *  
					 */

					var left = root.Parts[i - 1];
					var op = root.Parts[i];
					var right = root.Parts[i + 1];
					Block child = new();
					root.Parts.Insert(i, child);
					root.Parts.RemoveRange(i + 1, 2);
					root.Parts.RemoveAt(i - 1);
					child.Parent = root;
					child.Parts.AddRange(new[]{left, op, right});
					count -= 2;
					i -= 2;
				}
		}

		public override object Part2(string input)
		{
			return input
				.ToLines()
				.Select(x => {
					var tokens = Tokenize(x);
					var root = Blockify(tokens);
					ApplyAdvancedPrecedence(root);
					var result = Evaluate(root);
					return result;
				})
				.Sum();
		}
	}
}
