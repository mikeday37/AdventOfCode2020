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

namespace AdventOfCode2020.Challenges.Day24
{
	[Challenge(24, "Lobby Layout")]
	public class Day24Challenge : ChallengeBase
	{
		/*
		 * Coordinate Scheme:  (u,v)
		 *        _-_
		 *       | o | -> u  (East)
		 *        -_-
		 *		      \
		 *             v
		 *            (SouthEast)
		 */

		public enum HexDirection {None = 0, East = 1, SouthEast, SouthWest, West, NorthWest, NorthEast};
		public static readonly IReadOnlyDictionary<HexDirection, (int du, int dv)> OffsetMap = new Dictionary<HexDirection, (int du, int dv)>{
			[HexDirection.East]      = (  1,  0 ),
			[HexDirection.SouthEast] = (  0,  1 ),
			[HexDirection.SouthWest] = ( -1,  1 ),
			[HexDirection.West]      = ( -1,  0 ),
			[HexDirection.NorthWest] = (  0, -1 ),
			[HexDirection.NorthEast] = (  1, -1 )
		};

		public static IEnumerable<HexDirection> ToHexDirections(string line)
		{
			for (int i = 0; i < line.Length; i++)
				switch (line[i])
				{
					case 'e': yield return HexDirection.East; break;
					case 'w': yield return HexDirection.West; break;

					case 'n' when line[i + 1] == 'e': yield return HexDirection.NorthEast; i++; break;
					case 'n' when line[i + 1] == 'w': yield return HexDirection.NorthWest; i++; break;
					case 's' when line[i + 1] == 'e': yield return HexDirection.SouthEast; i++; break;
					case 's' when line[i + 1] == 'w': yield return HexDirection.SouthWest; i++; break;

					default: throw new Exception($"Invalid character(s) at index {i} in line: {line}");
				}
		}

		public override object Part1(string input)
		{
			HashSet<(int u, int v)> blackTiles = new();
			void Flip((int u, int v) pos)
			{
				if (blackTiles.Contains(pos))
					blackTiles.Remove(pos);
				else
					blackTiles.Add(pos);
			}

			foreach (var line in input.ToLines())
			{
				(int u, int v) = (0, 0);
				foreach (var hexdir in ToHexDirections(line))
				{
					var (du, dv) = OffsetMap[hexdir];
					u += du;
					v += dv;
				}
				Flip((u, v));
			}

			return blackTiles.Count;
		}

		public override object Part2(string input)
		{
			return -1;
		}
	}
}
