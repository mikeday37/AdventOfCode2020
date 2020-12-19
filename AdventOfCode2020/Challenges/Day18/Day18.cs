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
			 * 2. parenthesis tokens are only consecutive to other parenthesis tokens of the same direction,
			 *    or one value, without any intervening whitespace.
			 * 3. all other consecutive token pairs are separated by a single space.
			 * 4. if a space-split part has length > 1, it is a value either prefixed or suffixed with parentheses.
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
			public IReadOnlyList<object> Parts => parts;
			public void Add(Token t) => parts.Add(t);
			public Block Nest()
			{
				var child = new Block();
				child.Parent = this;
				parts.Add(child);
				return child;
			}

			private readonly List<object> parts = new();
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
			/*
			foreach (var line in input.ToLines())
				using (Logger.Context(line))
					foreach (var token in Tokenize(line))
						Logger.LogLine(token);
			*/

			return input
				.ToLines()
				.Select(x => {
					var tokens = Tokenize(x);
					var rootBlock = Blockify(tokens);
					var result = Evaluate(rootBlock);
					return result;
				})
				.Sum();
		}

		public override object Part2(string input)
		{
			return -1;
		}
	}
}
