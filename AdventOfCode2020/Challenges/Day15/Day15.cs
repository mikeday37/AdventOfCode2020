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

namespace AdventOfCode2020.Challenges.Day15
{
	[Challenge(15, "Rambunctious Recitation")]
	class Day15Challenge : ChallengeBase
	{
		private class MemoryGame
		{
			private readonly Dictionary<int, int> numberSpokenOnTurn = new();

			private int
				nextTurnNumber = 1,
				lastSpokenNumber = -1,
				lastSpokenTurnsApart = -1;
			private bool
				lastSpokenWasFirstTime = true;
			private ILogger Logger {get;}

			public MemoryGame(ILogger logger, IEnumerable<int> startingNumbers)
			{
				this.Logger = logger;

				using (Logger.Context("Init:"))
					foreach (var n in startingNumbers)
						Speak(n);

				Logger.LogLine("Initialized.");
			}

			public static IReadOnlyList<int> ParseInput(string input)
			{
				var startingNumbers = input.ToLines().Single().Split(',').Select(x => int.Parse(x)).ToList();
				return startingNumbers;
			}

			private void Speak(int n)
			{
				if (nextTurnNumber % 100000 == 0)
					Logger.LogLine($"Turn: {nextTurnNumber:D5}, Speak: {n}");

				lastSpokenNumber = n;

				if (numberSpokenOnTurn.TryGetValue(n, out var turn))
				{
					lastSpokenWasFirstTime = false;
					lastSpokenTurnsApart = nextTurnNumber - turn;
				}
				else
				{
					lastSpokenWasFirstTime = true;
					lastSpokenTurnsApart = 0;
				}

				numberSpokenOnTurn[n] = nextTurnNumber++;
			}

			public void SpeakTheNeedful()
			{
				if (lastSpokenWasFirstTime)
					Speak(0);
				else
					Speak(lastSpokenTurnsApart);
			}

			public int NextTurnNumber => nextTurnNumber;
			public int LastSpokenNumber => lastSpokenNumber;
		}

		int Play(string input, int turns)
		{
			var game = new MemoryGame(Logger, MemoryGame.ParseInput(input));

			using (Logger.Context("Playing to {turns} turns..."));
			while (game.NextTurnNumber <= turns)
				game.SpeakTheNeedful();
			var answer = game.LastSpokenNumber;
			Logger.LogLine("Done.  Answer = {answer}.");
			return answer;
		}

		public override object Part1(string input)
		{
			return Play(input, 2020);
		}

		public override object Part2(string input)
		{
			return Play(input, 30000000);
		}
	}
}
