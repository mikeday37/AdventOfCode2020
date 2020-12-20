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

namespace AdventOfCode2020.Challenges.Day20
{
	[Challenge(20, "Jurassic Jigsaw")]
	class Day20Challenge : ChallengeBase
	{
		public enum EdgePlacement { None = 0, Top, Left, Right, Bottom };

		public record Edge
		{
			public Tile Tile { get; init; }
			public ulong TileID => Tile.ID;
			public EdgePlacement Placement { get; init; }
			public int Natural { get; init; } // left to right or top to bottom
			public int Reversed { get; init; } // right ot left or bottom to top

			public override string ToString() => $"[{TileID}.{Placement.ToString()[0]}: {RawNatural}]";
			public string RawNatural => BitsToString(Natural);
			public string RawReversed => BitsToString(Reversed);

			public static string BitsToString(int n)
			{
				StringBuilder sb = new();
				for (var bit = 1 << 9; bit >= 1; bit >>= 1)
					sb.Append((n & bit) == 0 ? '.' : '#');
				return sb.ToString();
			}

			public static Edge Parse(Tile tile, EdgePlacement placement)
			{
				var natural = CharsToBits(tile.YieldEdgeCharacters(placement));
				var reversed = CharsToBits(tile.YieldEdgeCharacters(placement).Reverse());
				return new Edge
				{
					Tile = tile,
					Placement = placement,
					Natural = natural,
					Reversed = reversed
				};
			}

			public static int CharsToBits(IEnumerable<char> chars)
			{
				int bit = 1;
				int total = 0;
				foreach (var c in chars.Reverse())
				{
					if (c == '#')
						total |= bit;
					bit <<= 1;
				}
				return total;
			}
		}

		public class Tile
		{
			public ulong ID { get; private set; }
			public IReadOnlyList<string> RawOriginalData => rawOriginalData;
			public IReadOnlyDictionary<EdgePlacement, Edge> Edges => edges;
			private readonly List<string> rawOriginalData = new();
			private readonly Dictionary<EdgePlacement, Edge> edges = new();

			public static IEnumerable<Tile> ParseAll(string fullInput)
			{
				Tile curTile = null;

				foreach (var line in fullInput.ToLines(omitBlanks: false))
					switch (line)
					{
						case var l when string.IsNullOrWhiteSpace(l):
							if (curTile != null)
							{
								if (curTile.rawOriginalData.Count != 10)
									throw new Exception($"Unexpected number of lines: {curTile.rawOriginalData.Count}");
								curTile.ProcessEdges();
								yield return curTile;
							}
							curTile = null;
							continue;

						case var l when l[0] == 'T':
							curTile = new Tile { ID = ulong.Parse(line[5..^1]) };
							continue;

						default:
							if (line.Length != 10)
								throw new Exception($"Unexpected line length: {line.Length}");
							curTile.rawOriginalData.Add(line);
							break;
					}
			}

			private void ProcessEdges()
			{
				foreach (var placement in new[] { EdgePlacement.Top, EdgePlacement.Left, EdgePlacement.Right, EdgePlacement.Bottom })
					edges[placement] = Edge.Parse(this, placement);
			}

			public IEnumerable<char> YieldEdgeCharacters(EdgePlacement placement)
			{
				switch (placement)
				{
					case EdgePlacement.Top:
						foreach (var c in rawOriginalData[0])
							yield return c;
						break;

					case EdgePlacement.Bottom:
						foreach (var c in rawOriginalData[^1])
							yield return c;
						break;

					case EdgePlacement.Left:
						foreach (var line in rawOriginalData)
							yield return line[0];
						break;

					case EdgePlacement.Right:
						foreach (var line in rawOriginalData)
							yield return line[^1];
						break;
				}
			}
		}

		public partial class TileAnalyzer
		{
			public IReadOnlyDictionary<ulong, Tile> Tiles => tiles;
			public IReadOnlyList<Tile> CornerTiles => cornerTiles;

			private readonly Dictionary<ulong, Tile> tiles;
			private readonly Dictionary<int, List<(Edge, bool)>> edgeValues;
			private readonly List<Tile> cornerTiles;

			public TileAnalyzer(string input)
			{
				tiles = Tile.ParseAll(input).ToDictionary(x => x.ID);

				// build dictionary of edgeValue => List<(Edge, reversed?)> (all edges with that values, regardless of orientation)
				edgeValues = new();
				void add(Edge edge, bool reversed)
				{
					var edgeValue = reversed ? edge.Reversed : edge.Natural;
					if (!edgeValues.ContainsKey(edgeValue))
						edgeValues[edgeValue] = new();
					edgeValues[edgeValue].Add((edge, reversed));
				}
				foreach (var tile in tiles.Values)
					foreach (var edge in tile.Edges.Values)
					{
						add(edge, false);
						add(edge, true);
					}

				// now get all tiles for which exactly two edges match only their own tile -> they must be the corners
				cornerTiles = TilesWithCountOfEdgesOnlyMatchingSelfEquals(2).ToList();
			}

			public IEnumerable<Tile> TilesWithCountOfEdgesOnlyMatchingSelfEquals(int matchCount)
			{
				return tiles.Values
					.Where(t => matchCount == t.Edges.Values.Count(e =>
						1 == edgeValues[e.Natural].Count
						&&
						1 == edgeValues[e.Reversed].Count
					));
			}
		}

		public override object Part1(string input)
		{
			return new TileAnalyzer(input)
				.CornerTiles
				.Select(x => x.ID)
				.Aggregate(1UL, (a, b) => a * b);
		}

		public override object Part2(string input)
		{
			var ta = new TileAnalyzer(input);
			ta.FindSeaMonsters();
			return ta.WaterRoughness;
		}

		public partial class TileAnalyzer
		{
			public int WaterRoughness => waterRoughness.Value;
			private int? waterRoughness = null;

			public void FindSeaMonsters()
			{
				// edge tiles are like corners but instead of 2, they have just one edge that only matches their tile.
				var edgeTiles = TilesWithCountOfEdgesOnlyMatchingSelfEquals(1).ToList();

				// interior tiles have no edges that only match their tile
				var interiorTiles = TilesWithCountOfEdgesOnlyMatchingSelfEquals(0).ToList();

				// start with one corner as the seed, then put all other corners and edge tiles into a bag
				var seed = cornerTiles[0];
				var bag = cornerTiles.Skip(1).Concat(edgeTiles).ToList();

				// start the board by placing the seed, oriented as needed, into position (0,0)
				Board board = new();
				board.PlaceFirstCorner(seed);

				// grow the seed into a finished border by adding from the bag until the bag is empty
				void PlaceAllFromBag()
				{
					while (bag.Any())
						foreach (var tile in bag)
							if (board.TryPlace(tile))
							{
								bag.Remove(tile);
								break;
							}
				}
				PlaceAllFromBag();

				// grow the border into the finished picture by refilling the bag with the interior pieces and repeating the above process
				bag = interiorTiles.ToList();
				PlaceAllFromBag();
			}

			public class PlacedTile
			{
			}

			public class Board
			{
				public void PlaceFirstCorner(Tile tile)
				{
				}

				public bool TryPlace(Tile tile)
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
