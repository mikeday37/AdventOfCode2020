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

namespace AdventOfCode2020.Challenges.Day22
{
	[Challenge(22, "Crab Combat")]
	public class Day22Challenge : ChallengeBase
	{
		private static Dictionary<int, Queue<int>> ParseInput(string input)
		{
			Dictionary<int, Queue<int>> decks = new();
			int player = 0;
			foreach (var line in input.ToLines())
				if (line[0] == 'P')
					decks[player = int.Parse(line[^2..^1])] = new();
				else
					decks[player].Enqueue(int.Parse(line));
			return decks;
		}

		public override object Part1(string input)
		{
			return PlayCombat(ParseInput(input)).score;
		}

		public override object Part2(string input)
		{
			return PlayCombat(ParseInput(input), recursive: true).score;
		}

		private (int winner, int score) PlayCombat(Dictionary<int, Queue<int>> deck, bool recursive = false)
		{
			int game = 1;
			return PlayCombat(ref game, deck, recursive);
		}

		/// <summary>
		/// Plays the generalized version of Day 22's "Combat" and "Recursive Combat" games...
		/// generalized in that there can be any number of players, not just two.  :)
		/// </summary>
		private (int winner, int score) PlayCombat(ref int nextGame, Dictionary<int, Queue<int>> deck, bool recursive)
		{
			var game = nextGame++;
			Logger.LogLine($"=== Game {game} ===");

			HashSet<string> history = new();
			void logAllDecks()
			{
				foreach (var p in deck.Keys)
					Logger.LogLine($"Player {p}'s deck: {string.Join(", ", deck[p])}");
			}

			int round = 0, winner;
			do
			{
				base.AllowCancel();
				var state = string.Join("; ", deck.OrderBy(x => x.Key).Select(x => $"{x.Key}: {string.Join(", ", x.Value)}"));
				if (!history.Add(state))
				{
					if (recursive)
					{
						winner = 1;
						Logger.LogLine("Infinite loop detected!  Player 1 wins due to Recursive Combat rules!");
						break;
					}
					else
						throw new Exception("Infinite loop detected!");
				}

				Logger.LogLine($"\n-- Round {++round} {(recursive ? $"(Game {game})" : "")}--");
				logAllDecks();
					
				var picks = deck
					.Where(x => x.Value.Any())
					.Select(x => (player: x.Key, card: x.Value.Dequeue()))
					.OrderBy(x => x.player)
					.ToList();
				foreach (var pick in picks)
					Logger.LogLine($"Player {pick.player} plays: {pick.card}");

				if (recursive && picks.Count > 1 && picks.All(x => x.card <= deck[x.player].Count))
				{
					Logger.LogLine("Playing a sub-game to determine the winner...\n");
					var subDeck = deck
						.ToDictionary(
							x => x.Key,
							x => x.Value
								.Take(picks
									.Where(p => p.player == x.Key)
									.Select(p => p.card)
									.SingleOrDefault()
								)
								.ToQueue()
						);
					winner = PlayCombat(ref nextGame, subDeck, true).winner;
					Logger.LogLine($"...anyway, back to game {game}.");
				}
				else
					winner = picks.OrderByDescending(x => x.card).First().player;

				Logger.LogLine(recursive
					? $"Player {winner} wins round {round} of game {game}!"
					: $"Player {winner} wins the round!"
				);

				foreach (var pick in picks.OrderBy(x => x.player == winner ? 1 : 2))
					deck[winner].Enqueue(pick.card);
			}
			while (deck.Values.Count(x => x.Any()) != 1);

			if (recursive)
				Logger.LogLine($"The winner of game {game} is player {winner}!");
			Logger.LogLine();

			if (game == 1)
			{
				Logger.LogLine("\n== Post-game results ==");
				logAllDecks();
			}

			return (
				winner: winner, 
				score: deck[winner]
					.Reverse()
					.WithIndex()
					.Sum(x => x.item * (1 + x.index))
			);
		}
	}
}
