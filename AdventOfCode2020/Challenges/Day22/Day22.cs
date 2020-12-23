﻿using AdventOfCodeScaffolding;
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
		public override object Part1(string input)
		{
			int game = 1;
			bool recurse = false;
			Dictionary<int, Queue<int>> deck = new();
			int player = 0;
			foreach (var line in input.ToLines())
				if (line[0] == 'P')
					deck[player = int.Parse(line[^2..^1])] = new();
				else if (line == "R")
					recurse = true;
				else
					deck[player].Enqueue(int.Parse(line));

			return PlayCombat(0, ref game, deck, recurse).score;
		}

		public override object Part2(string input)
		{
			return Part1("R\n" + input);
		}

		public (int winner, int score) PlayCombat(int parentGame, ref int nextGame, Dictionary<int, Queue<int>> deck, bool recursive)
		{
			var game = nextGame++;
			Logger.LogLine($"=== Game {game} ===\n");

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

				Logger.LogLine($"-- Round {++round} --");
				logAllDecks();
					
				var picks = deck
					.Where(x => x.Value.Any())
					.Select(x => (player: x.Key, card: x.Value.Dequeue()))
					.OrderByDescending(x => x.card)
					.ToList();
				foreach (var pick in picks.OrderBy(x => x.player))
					Logger.LogLine($"Player {pick.player} plays: {pick.card}");

				winner = picks.First().player;
				Logger.LogLine(recursive
					? $"Player {winner} wins round {round} of game {game}!\n"
					: $"Player {winner} wins the round!\n"
				);

				foreach (var pick in picks)
					deck[winner].Enqueue(pick.card);
			}
			while (deck.Values.Count(x => x.Any()) != 1);

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
