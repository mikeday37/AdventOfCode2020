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

namespace AdventOfCode2020.Challenges.Day17
{
	[Challenge(17, "Conway Cubes")]
	class Day17Challenge : ChallengeBase
	{
		public record InclusiveRange
		{
			public int Min {get; init;}
			public int Max {get; init;}

			public InclusiveRange(int min, int max)
			{
				if (min <= max)
					(Min, Max) = (min, max);
				else
					throw new Exception($"InclusiveRange must be created with min <= max.  Attempted min = {min}, max = {max}.");
			}

			public InclusiveRange((int, int) t) : this(t.Item1, t.Item2) {}

			public int Size => 1 + Max - Min;

			public bool IsValid(int i) => Min <= i && i <= Max;

			public IEnumerable<int> YieldAll()
			{
				for (int i = Min; i <= Max; i++)
					yield return i;
			}

			public IEnumerable<int> YieldInner()
			{
				for (int i = Min + 1; i <= Max - 1; i++)
					yield return i;
			}
		}

		public class NDimensionalGrid<T>
		{
			public int NumDimensions { get; }
			public IReadOnlyList<InclusiveRange> Extent { get; }
			public int CellCount { get; }

			private readonly T[] grid;

			public T this[int i]
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
				get { ValidateIndex(i); return grid[i]; }

				[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
				set { ValidateIndex(i); grid[i] = value; }
			}

			public T this[IReadOnlyList<int> coordinates]
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
				get { ValidateCoordinates(coordinates); return grid[ToIndex(coordinates)]; }

				[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
				set { ValidateCoordinates(coordinates); grid[ToIndex(coordinates)] = value; }
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			public void ValidateIndex(int index)
			{
				if (index < 0 || index >= CellCount)
					throw new ArgumentOutOfRangeException(nameof(index));
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			public void ValidateCoordinates(IReadOnlyList<int> coordinates)
			{
				if (coordinates == null)
					throw new ArgumentNullException(nameof(coordinates));
				
				if (coordinates.Count != NumDimensions)
					throw new ArgumentException($"coordinates.Count must equal NumDimensions.  Bad count = {coordinates.Count}.", nameof(coordinates));

				for (int i = 0; i < NumDimensions; i++)
					if (!Extent[i].IsValid(coordinates[i]))
						throw new ArgumentException($"Coordinate index {i} = {coordinates[i]} is outside the allowed extent of (Min, Max) = ({Extent[i].Min}, {Extent[i].Max}).", nameof(coordinates));
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			public int ToIndex(IReadOnlyList<int> coordinates)
			{
				ValidateCoordinates(coordinates);

				var z = NumDimensions - 1;
				int index = coordinates[z] - Extent[z].Min;
				for (int i = z - 1; i >= 0; i--)
				{
					index *= Extent[i].Size;
					index += coordinates[i] - Extent[i].Min;
				}

				return index;
			}

			public NDimensionalGrid(int numDimensions, IReadOnlyList<InclusiveRange> extent)
			{
				if (numDimensions < 1)
					throw new ArgumentOutOfRangeException(nameof(numDimensions), "numDimensions must be at least 1.");

				if (extent == null)
					throw new ArgumentNullException(nameof(extent));

				if (extent.Count != numDimensions)
					throw new ArgumentException("extent.Count must equal numDimensions.", nameof(extent));

				NumDimensions = numDimensions;
				Extent = extent.ToArray();
				CellCount = extent.Select(x => x.Size).Aggregate((a,b)=>a*b);
				grid = new T[CellCount];
			}

			public NDimensionalGrid(int numDimensions, IReadOnlyList<(int, int)> extent) : this(numDimensions, extent.Select(x => new InclusiveRange(x)).ToList()) {}
		}

		public void OutputGrid(NDimensionalGrid<bool> grid)
		{
			using (Logger.Context("Grid State:"))
			{
				var (xe, ye, ze) = (grid.Extent[0], grid.Extent[1], grid.Extent[2]);
				foreach (var z in ze.YieldInner())
					using (Logger.Context($"Z = {z}:"))
						foreach (var y in ye.YieldInner())
						{	
							StringBuilder sb = new();
							foreach (var x in xe.YieldInner())
								sb.Append(grid[new[]{x,y,z}] ? '#' : '.');
							Logger.LogLine(sb.ToString());
						}
			}
		}

		IEnumerable<int> NeighborOffsets(NDimensionalGrid<bool> grid)
		{
			var originIndex = grid.ToIndex(new[]{0,0,0});
			for (int x = -1; x <= 1; x++)
				for (int y = -1; y <= 1; y++)
					for (int z = -1; z <= 1; z++)
					{
						if (x == 0 && y == 0 && z == 0)
							continue;
						yield return grid.ToIndex(new[]{x,y,z})-originIndex;
					}
		}

		NDimensionalGrid<bool> Iterate(NDimensionalGrid<bool> grid)
		{
			var old = grid;
			grid = new NDimensionalGrid<bool>(old.NumDimensions, old.Extent.Select(x => new InclusiveRange((x.Min - 1, x.Max + 1))).ToList());
			var sourceOffsets = NeighborOffsets(old).ToArray();

			foreach (var z in old.Extent[2].YieldInner())
				foreach (var y in old.Extent[1].YieldInner())
					foreach (var x in old.Extent[0].YieldInner())
					{
						var currentCoordinates = new[]{x,y,z};
						var currentSourceIndex = old.ToIndex(currentCoordinates);
						int active = 0;
						for (int i = 0; i < 26; i++)
							if (old[currentSourceIndex + sourceOffsets[i]])
								active++;
						if (old[currentSourceIndex])
							grid[currentCoordinates] = active == 2 || active == 3;
						else
							grid[currentCoordinates] = active == 3;
					}

			return grid;
		}

		public override object Part1(string rawInput)
		{
			var lines = rawInput.ToLines().ToList();
			var initialHeight = lines.Count;
			var initialWidth = lines[0].Length;

			var grid = new NDimensionalGrid<bool>(3, new[]{(-1,initialWidth),(-1,initialHeight),(-2,2)} );

			var z = 0;
			for (int y = 0; y < initialHeight; y++)
			{
				var line = lines[y];
				for (int x = 0; x < initialWidth; x++)
					grid[new[]{x,y,z}] = line[x] == '#';
			}
			
			using (Logger.Context("\nInitial State:"))
				OutputGrid(grid);

			foreach (var n in Enumerable.Range(1, 6))
			{
				grid = Iterate(grid);
				using (Logger.Context($"\nAfter {n} cycles:"))
					OutputGrid(grid);
			}

			return -1;
		}
	}
}
