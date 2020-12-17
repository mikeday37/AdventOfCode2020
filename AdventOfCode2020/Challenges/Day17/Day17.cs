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

		public override object Part1(string rawInput)
		{
			var g = new NDimensionalGrid<bool>(3, new[]{(0,2),(0,3),(0,1)} );

			void t(int x, int y, int z)
			{
				var c = new int[3]{x,y,z};
				var index = g.ToIndex(c);
				Logger.LogLine($"({string.Join(",",c)}) -> {index}");
			}

			t(0,0,0);
			t(2,3,1);
			t(2,0,0);
			t(1,1,0);
			t(0,2,1);

			return -1;
		}
	}
}
