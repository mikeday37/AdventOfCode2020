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

namespace AdventOfCode2020.Challenges.Day25
{
	[Challenge(25, "Combo Breaker")]
	public class Day25Challenge : ChallengeBase
	{
		public class Analyzer
		{
			public IReadOnlyList<long> PublicKeys { get; }
			public long EncryptionKey { get; }

			public Analyzer(string input)
			{
				PublicKeys = input.ToLines().Select(long.Parse).ToArray();
				if (PublicKeys.Count != 2)
					throw new Exception($"Unexpected number of public keys: {PublicKeys.Count}");
				EncryptionKey = FindEncryptionKey();
			}

			/*
			 * Transform:
			 *		v = 1
			 *		repeat <loop size>
			 *			v = (v * <subject number>) % 20201227
			 *		return v
			 *			
			 * Handshake:
			 *		card's PK = transform(7, <card loop size>)
			 *		door's PK = transform(7, <door loop size>)
			 *		share each with the other
			 *		
			 *		card's encryption key = transform(door's PK, <card loop size>)
			 *		door's encryption key = transform(card's PK, <door loop size>)
			 *		(they should match)
			 */

			public static long Transform(long subjectNumber, long loopSize)
			{
				long v = 1;
				for (var i = 0; i < loopSize; i++)
					v = (v * subjectNumber) % 20201227;
				return v;
			}

			public static long FindLoopSize(long publicKey)
			{
				long loopSize = 0;
				long v = 1;
				do
				{
					++loopSize;
					if (loopSize % 10000 == 0)
						ThreadLogger.LogLine($"FindLoopSize({publicKey}) @{loopSize}");
					v = (v * 7) % 20201227;
				}
				while (v != publicKey);
				return loopSize;
			}

			public long FindEncryptionKey()
			{
				var firstLoopSize = FindLoopSize(PublicKeys[0]);
				ThreadLogger.LogLine($"firstLoopSize = {firstLoopSize}");

				var encryptionKey = Transform(PublicKeys[1], firstLoopSize);
				ThreadLogger.LogLine($"encryptionKey = {encryptionKey}");

				return encryptionKey;
			}
		}

		public override object Part1(string input)
		{
			return new Analyzer(input).EncryptionKey;
		}

		public override object Part2(string input)
		{
			return -1;
		}
	}
}
