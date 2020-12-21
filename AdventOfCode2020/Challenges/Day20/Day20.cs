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
	public class Day20Challenge : ChallengeBase
	{
		public enum EdgePlacement { None = 0, Right = 1, Bottom = 2, Left = 3, Top = 4}; // these values and their order are crucial

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
			private readonly Dictionary<int, List<(Edge edge, bool reversed)>> edgeValues;
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

		private static readonly IReadOnlyList<string> seaMonster = new[]{
				"..................#.",
				"#....##....##....###", // its name is Nessy.  :)
				".#..#..#..#..#..#..."
			};

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
				// assumption: there's at least one nessy (the sea monster)

				var picture = SolvePuzzle();
				var nessy = seaMonster.ToList();

				PictureUtility.LogLines("Assembled Photo:", picture.Lines);

				using (ChallengeBase.ThreadLogger.Context("Searching for Sea Monsters..."))
				for (int attempt = 1; attempt <= 8; attempt++)
				{
					// flip vertically after the first set of rotations.
					if (attempt == 5)
						nessy.Reverse();

					PictureUtility.LogLines($"Attempt {attempt}:", nessy);

					if (picture.RemoveNessies(nessy))
					{
						PictureUtility.LogLines("Found!\nRemoved from Photo:", picture.Lines);
						waterRoughness = picture.CalculateWaterRoughness();
						return;
					}

					nessy = PictureUtility.RotateClockwise(nessy);
				}

				throw new Exception("Could not find any sea monsters!");
			}

			public class Picture
			{
				public IReadOnlyList<string> Lines => lines;
				private List<string> lines = new();

				public Picture(Board board)
				{
					foreach (var row in Enumerable.Range(0, board.Height))
					{
						var sba = Enumerable.Range(0, 8).Select(x => new StringBuilder()).ToArray();
						foreach (var col in Enumerable.Range(0, board.Width))
							board.Grid[(row, col)].Render(sba);
						foreach (var sb in sba)
							lines.Add(sb.ToString());
					}
				}

				/// <summary>
				/// Simultenously removes all discovered nessies from the picture, then returns true if any were removed.
				/// </summary>
				/// <param name="nessy">The nessy to search for and remove.</param>
				/// <returns>True if any nessies were removed, false if not.</returns>
				public bool RemoveNessies(IReadOnlyList<string> nessy)
				{
					var (nw, nh, w, h) = ( // nessy dimensions, picture dimensions
						nessy[0].Length, nessy.Count,
						lines[0].Length, lines.Count);

					IEnumerable<(int r, int c)> NessyPartOffsets()
					{
						foreach (var nr in nessy.WithIndex())
							foreach (var nc in nr.item.WithIndex().Where(x => x.item == '#'))
								yield return (r: nr.index, c: nc.index);
					}
					var npo = NessyPartOffsets().ToList();

					int count = 0;
					HashSet<(int r, int c)> remove = new();
					for (int r = 0; r < h - nh; r++)
						for (int c = 0; c < w - nw; c++)
						{
							if (npo.All(po => lines[r + po.r][c + po.c] == '#'))
							{
								count++;
								foreach (var po in npo)
									remove.Add((r: r + po.r, c: c + po.c));
							}
						}

					foreach (var x in remove)
					{
						var ca = lines[x.r].ToCharArray();
						ca[x.c] = 'O';
						lines[x.r] = new string(ca);
					}
					
					return count > 0;
				}

				public int CalculateWaterRoughness()
				{
					return lines.Select(x => x.Count(y => y == '#')).Sum();
				}
			}

			Picture SolvePuzzle()
			{
				// edge tiles are like corners but instead of 2, they have just one edge that only matches their tile.
				var edgeTiles = TilesWithCountOfEdgesOnlyMatchingSelfEquals(1).ToList();

				// interior tiles have no edges that only match their tile
				var interiorTiles = TilesWithCountOfEdgesOnlyMatchingSelfEquals(0).ToList();

				// start with one corner as the seed, then put all other tiles into a bag
				var seed = cornerTiles[0];
				var bag = cornerTiles
					.Skip(1)
					.Select(x => (tile: x, kind: PlacedTileKind.Corner))
					.Concat(edgeTiles.Select(x => (tile: x, kind: PlacedTileKind.Edge)))
					.Concat(interiorTiles.Select(x => (tile: x, kind: PlacedTileKind.Interior)))
					.ToDictionary(x => x.tile.ID);

				// start the board by placing the seed, oriented as needed, into position (0,0)
				Board board = new(this);
				board.PlaceUpperLeftCorner(seed);

				// grow the seed by placing from bag against unmet edges until none remain
				do
				{
					// get one unmet edgeValue
					var unmetEdge = board.UnmetEdges.First().Value;

					// get (the) one potential tile id it matches that is still in the bag
					var tileID = edgeValues[unmetEdge.TrueEdgeValue]
						.Select(x => x.Item1.TileID)
						.Distinct()
						.Where(x => bag.ContainsKey(x))
						.Single();
					var tile = tiles[tileID];

					// place tile then remove from bag
					board.Place(tile, bag[tileID].kind, unmetEdge);
					bag.Remove(tileID);
				}
				while (board.UnmetEdges.Any());

				// now build the picture and return it
				return new Picture(board);
			}

			public record PlacedEdge
			{
				public Edge OriginalEdge {get; init;}
				public PlacedTile PlacedTile {get; init;}
				public bool Reversed {get; init;}
				public EdgePlacement TruePlacement {get; init;}
				public int TrueEdgeValue => Reversed ? OriginalEdge.Reversed : OriginalEdge.Natural;
			}

			public enum PlacedTileKind {None = 0, Corner, Edge, Interior};

			public record PlacedTile
			{
				public Tile OriginalTile {get; init;}
				public PlacedTileKind Kind {get; init;}

				/// <summary>
				/// True if the placement of the tile requires it to be flipped vertically before any rotation.
				/// </summary>
				public bool Flipped {get; init;}

				/// <summary>
				/// If non-zero, the placement of the tile requires it to be rotated clockwise this number of times after potential flipping.
				/// </summary>
				public int Rotations {get; init;}

				public int Row {get; init;}
				public int Column {get; init;}

				public (int row, int column) Location => (Row, Column);

				/// <summary>
				/// Get's a dictionary of placed edges by their true placement.
				/// </summary>
				public IReadOnlyDictionary<EdgePlacement, PlacedEdge> PlacedEdges => placedEdges.Value;

				public PlacedTile() => placedEdges = new(() => CalculatePlacedEdges.ToDictionary(x => x.TruePlacement));
				private readonly Lazy<IReadOnlyDictionary<EdgePlacement, PlacedEdge>> placedEdges;
				private PlacedEdge[] CalculatePlacedEdges {get{
					var cur = (
						right:  (edge: OriginalTile.Edges[EdgePlacement.Right],  reversed: false),
						bottom: (edge: OriginalTile.Edges[EdgePlacement.Bottom], reversed: false),
						left:   (edge: OriginalTile.Edges[EdgePlacement.Left],   reversed: false),
						top:    (edge: OriginalTile.Edges[EdgePlacement.Top],    reversed: false)
					);
						if (Flipped)
							cur = FlipVertically(cur);
						foreach (var _ in Enumerable.Range(0, Rotations))
							cur = RotateClockwise(cur);
						return new[]{
							(result: cur.right,  truePlacement: EdgePlacement.Right),
							(result: cur.bottom, truePlacement: EdgePlacement.Bottom),
							(result: cur.left,   truePlacement: EdgePlacement.Left),
							(result: cur.top,    truePlacement: EdgePlacement.Top)
						}
						.Select(x => new PlacedEdge{
							OriginalEdge = x.result.edge,
							PlacedTile = this,
							Reversed = x.result.reversed,
							TruePlacement = x.truePlacement
						})
						.ToArray();
				}}

				public static ((T edge, bool reversed) right, (T edge, bool reversed) bottom, (T edge, bool reversed) left, (T edge, bool reversed) top) 
					FlipVertically<T>(((T edge, bool reversed) right, (T edge, bool reversed) bottom, (T edge, bool reversed) left, (T edge, bool reversed) top) cur)
				{
					return (
						right:  (cur.right.edge,  !cur.right.reversed),
						bottom: (cur.top.edge,     cur.top.reversed),
						left:   (cur.left.edge,   !cur.left.reversed),
						top:    (cur.bottom.edge,  cur.bottom.reversed)
					);
				}

				public static ((T edge, bool reversed) right, (T edge, bool reversed) bottom, (T edge, bool reversed) left, (T edge, bool reversed) top)
					RotateClockwise<T>(((T edge, bool reversed) right, (T edge, bool reversed) bottom, (T edge, bool reversed) left, (T edge, bool reversed) top) cur)
				{
					return (
						right:  (cur.top.edge,     cur.top.reversed),
						bottom: (cur.right.edge,  !cur.right.reversed),
						left:   (cur.bottom.edge,  cur.bottom.reversed),
						top:    (cur.left.edge,   !cur.left.reversed)
					);
				}

				internal void Render(StringBuilder[] sba)
				{
					var logger = ChallengeBase.ThreadLogger;
					using (logger.Context($"Render {OriginalTile.ID}:"))
					{
						using (logger.Context("Tile Info:"))
						{
							logger.LogLine($"Location:  {Location}");
							logger.LogLine($"Flipped:   {Flipped}");
							logger.LogLine($"Rotations: {Rotations}");
						}

						PictureUtility.LogLines("RawOriginalData:", OriginalTile.RawOriginalData);

						var lines = OriginalTile.RawOriginalData.Skip(1).SkipLast(1).Select(x => x[1..^1]).ToList();

						if (Flipped)
							lines.Reverse();
						foreach (var _ in Enumerable.Range(0, Rotations))
							lines = PictureUtility.RotateClockwise(lines);

						PictureUtility.LogLines("Output:", lines);

						foreach (var (item, index) in lines.WithIndex())
							sba[index].Append(item);
					}
				}
			}

			public static class PictureUtility
			{
				public static void LogLines(string context, IEnumerable<string> lines)
				{
					var logger = ChallengeBase.ThreadLogger;
					using (logger.Context(context))
						foreach (var line in lines)
							logger.LogLine(line);
				}

				public static List<string> RotateClockwise(List<string> lines)
				{
					// assumption: all lines are the same length, no null, at least one.

					var (ow, oh) = (lines[0].Length, lines.Count);
					if (!lines.Skip(1).All(x => x.Length == ow))
						throw new Exception("Invalid lines for rotation.");

					var sba = Enumerable.Range(0, ow).Select(x => new StringBuilder()).ToArray();
					for (int oc = 0; oc < ow; oc++)
						for (int or = oh - 1; or >= 0; or--)
							sba[oc].Append(lines[or][oc]);

					return sba.Select(x => x.ToString()).ToList();
				}
			}

			public class Board
			{
				public TileAnalyzer TileAnalyzer {get;}
				public Board(TileAnalyzer tileAnalyzer) => TileAnalyzer = tileAnalyzer;

				public IReadOnlyDictionary<(int row, int column), PlacedTile> Grid => grid;
				private readonly Dictionary<(int row, int column), PlacedTile> grid = new();

				public int Width {get; private set;}
				public int Height {get; private set;}

				public IReadOnlyDictionary<int, PlacedEdge> UnmetEdges => unmetEdges;
				private readonly Dictionary<int, PlacedEdge> unmetEdges = new();

				public void PlaceUpperLeftCorner(Tile tile)
				{
					// determine which corners only match itself
					List<Edge> cornerEdges = new();
					foreach (var edge in tile.Edges.Values)
						if (
								1 == TileAnalyzer.edgeValues[edge.Natural].Count
									&&
								1 == TileAnalyzer.edgeValues[edge.Reversed].Count
							)
							cornerEdges.Add(edge);
							
					// get the placements in an ordered array
					var orderedCornerPlacements = cornerEdges.Select(x => x.Placement).OrderBy(x => x).ToArray();

					// this will yield one of the following lists of placements:
					//   RB = needs 2 clockwise rotations         T
					//   BL = needs 1                         ^  L R1 |
					//   LT = needs 0                         |   B   v
					//   RT = needs 3                            <--

					// figure the number of clockwise rotations required
					var clockwiseRotationsRequired =
						((int)orderedCornerPlacements[1] - (int)orderedCornerPlacements[0] > 1) // the special RT case
							? 3
							: 3 - (int)orderedCornerPlacements[0];

					// place the tile (we don't have to flip the first tile)
					var seed = grid[(0,0)] = new PlacedTile{
						OriginalTile = tile,
						Kind = PlacedTileKind.Corner,
						Flipped = false,
						Rotations = clockwiseRotationsRequired,
						Row = 0,
						Column = 0
					}; seed.LogPlacement();

					// we start at 1 by 1
					Width = Height = 1;

					// the unmet edge values are the placed tile's true right and bottom true edge values
					foreach (var unmetEdge in new []{EdgePlacement.Right, EdgePlacement.Bottom}.Select(x => seed.PlacedEdges[x]))
					{
						if (unmetEdges.ContainsKey(unmetEdge.TrueEdgeValue))
							throw new Exception("Unmet edge collision adding first corner.");
						unmetEdges[unmetEdge.TrueEdgeValue] = unmetEdge;
					}
				}

				public static IReadOnlyDictionary<
						(EdgePlacement originalPlacement, EdgePlacement requiredPlacement, bool requiresReversal),
						(bool flip, int clockwiseRotations)> 
					OrientationMap => orientationMap.Value;
				private static readonly Lazy<IReadOnlyDictionary<
						(EdgePlacement originalPlacement, EdgePlacement requiredPlacement, bool requiresReversal),
						(bool flip, int clockwiseRotations)>>
					orientationMap = new(()=>CalculationOrientationMap());
				private static IReadOnlyDictionary<
						(EdgePlacement originalPlacement, EdgePlacement requiredPlacement, bool requiresReversal),
						(bool flip, int clockwiseRotations)>
					CalculationOrientationMap()
				{
					Dictionary<
						(EdgePlacement originalPlacement, EdgePlacement requiredPlacement, bool requiresReversal),
						(bool flip, int clockwiseRotations)
					> map = new();

					var cur = (
						right:  (edge: EdgePlacement.Right,  reversed: false),
						bottom: (edge: EdgePlacement.Bottom, reversed: false),
						left:   (edge: EdgePlacement.Left,   reversed: false),
						top:    (edge: EdgePlacement.Top,    reversed: false)
					);
					var flipped = PlacedTile.FlipVertically(cur);

					foreach (var n in Enumerable.Range(0, 4))
					{
						map[(originalPlacement: cur.right.edge,  requiredPlacement: EdgePlacement.Right,  requiresReversal: cur.right.reversed)]  = (flip: false, clockwiseRotations: n);
						map[(originalPlacement: cur.bottom.edge, requiredPlacement: EdgePlacement.Bottom, requiresReversal: cur.bottom.reversed)] = (flip: false, clockwiseRotations: n);
						map[(originalPlacement: cur.left.edge,   requiredPlacement: EdgePlacement.Left,   requiresReversal: cur.left.reversed)]   = (flip: false, clockwiseRotations: n);
						map[(originalPlacement: cur.top.edge,    requiredPlacement: EdgePlacement.Top,    requiresReversal: cur.top.reversed)]    = (flip: false, clockwiseRotations: n);

						map[(originalPlacement: flipped.right.edge,  requiredPlacement: EdgePlacement.Right,  requiresReversal: flipped.right.reversed)]  = (flip: true, clockwiseRotations: n);
						map[(originalPlacement: flipped.bottom.edge, requiredPlacement: EdgePlacement.Bottom, requiresReversal: flipped.bottom.reversed)] = (flip: true, clockwiseRotations: n);
						map[(originalPlacement: flipped.left.edge,   requiredPlacement: EdgePlacement.Left,   requiresReversal: flipped.left.reversed)]   = (flip: true, clockwiseRotations: n);
						map[(originalPlacement: flipped.top.edge,    requiredPlacement: EdgePlacement.Top,    requiresReversal: flipped.top.reversed)]    = (flip: true, clockwiseRotations: n);

						cur = PlacedTile.RotateClockwise(cur);
						flipped = PlacedTile.RotateClockwise(flipped);
					}

					return map;
				}

				public void Place(Tile tile, PlacedTileKind kind, PlacedEdge destEdge)
				{
					// first determine the coordinates of the new tile, based on:
					//    1. the coordinates of the neighbor
					//    2. the true placement of the destination edge
					// along the way we can also conveniently determine the true placement for the new tile's attached edge

					var neighbor = destEdge.PlacedTile;
					var newLocation = neighbor.Location;
					EdgePlacement newTileAttachedEdgeTruePlacement;
					switch (destEdge.TruePlacement)
					{
						case EdgePlacement.Right:  newLocation.column++; newTileAttachedEdgeTruePlacement = EdgePlacement.Left;   break;
						case EdgePlacement.Bottom: newLocation.row++;    newTileAttachedEdgeTruePlacement = EdgePlacement.Top;    break;
						case EdgePlacement.Left:   newLocation.column--; newTileAttachedEdgeTruePlacement = EdgePlacement.Right;  break;
						case EdgePlacement.Top:    newLocation.row--;    newTileAttachedEdgeTruePlacement = EdgePlacement.Bottom; break;
						default: throw new Exception($"Unexpected placement: {destEdge.TruePlacement}");
					}

					// git the edge of the tile that matches the destination edge
					var newTileAttachedEdgeInfo = tile
						.Edges
						.Values
						.SelectMany(x => new[]{
							(edgeValue: x.Natural, edge: x, reversed: false),
							(edgeValue: x.Reversed, edge: x, reversed: true)
						})
						.Single(x => destEdge.TrueEdgeValue == x.edgeValue);

					// look up what we have to do to orient the new tile's original edge to its required orientation
					var (flip, clockwiseRotations) = OrientationMap[(
							originalPlacement: newTileAttachedEdgeInfo.edge.Placement,
							requiredPlacement: newTileAttachedEdgeTruePlacement,
							requiresReversal: newTileAttachedEdgeInfo.reversed
						)];;

					// place the new tile
					var newTile = grid[newLocation] = new PlacedTile{
						OriginalTile = tile,
						Kind = kind,
						Flipped = flip,
						Rotations = clockwiseRotations,
						Row = newLocation.row,
						Column = newLocation.column
					}; newTile.LogPlacement();

					// extend dimensions accordingly
					if (newLocation.row >= Width)
						Width++;
					if (newLocation.column >= Height)
						Height++;

					// for each newly placed edge, either remove it from unmet, or add it to unmet if it's not exterior
					foreach (var placedEdge in newTile.PlacedEdges.Values)
					{
						if (unmetEdges.ContainsKey(placedEdge.TrueEdgeValue))
							unmetEdges.Remove(placedEdge.TrueEdgeValue);
						else
						{
							bool isExterior = TileAnalyzer.edgeValues[placedEdge.TrueEdgeValue].All(x => x.edge.TileID == tile.ID);
							if (!isExterior)
								unmetEdges[placedEdge.TrueEdgeValue] = placedEdge;
						}
					}
				}
			}
		}
	}

	public static class LocalExtensions
	{
		public static void LogPlacement(this Day20Challenge.TileAnalyzer.PlacedTile tile)
		{
			ChallengeBase.ThreadLogger.LogLine(
				$"{tile.Kind,8} tile {tile.OriginalTile.ID} placed at {tile.Location,8} with {tile.Rotations} clockwise rotation(s){(tile.Flipped ? ", after flipping vertically" : "")}."
			);
		}
	}
}
