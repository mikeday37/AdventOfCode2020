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
		public enum TokenType {Add, Mult, LeftParen, RightParen, Value};

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

		public override object Part1(string input)
		{
			foreach (var line in input.ToLines())
				using (Logger.Context(line))
					foreach (var token in Tokenize(line))
						Logger.LogLine(token);

			return -1;
		}

		public override object Part2(string input)
		{
			return -1;
		}
	}
}
