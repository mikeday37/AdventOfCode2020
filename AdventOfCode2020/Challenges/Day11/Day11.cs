using AdventOfCodeScaffolding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges.Day11
{
	[Challenge(11, "Seating System")]
	class Day11Challenge : ChallengeBase
	{
		/*
		 *   . means no seat, doesn't change
		 *   L, oa == 0 -> #
		 *   #, oa >= 4 -> L
		 *   
		 *   part 1: return Count(#) when grid is stable.
		 *   
		 *   part 2: same, but 4->5, and we're not checking adjacent, but "first visible" in each of the 8 directions.
		 * 
		 */

		public class SeatingArea
		{
			public int Width {get;}
			public int Height {get;}
			public int Tolerance {get;}
			public bool FirstVisible {get;}

			public static SeatingArea Parse(string input, int part)
			{
				var lines = input.ToLines().ToList();
				var width = lines.First().Length;
				var height = lines.Count;

				var sa = new SeatingArea(width, height, part);

				foreach (var (line, y) in lines.WithIndex())
					foreach (var (c, x) in line.WithIndex())
						sa.grid[x,y] = c;

				return sa;
			}

			/// <summary>
			/// Advance the entire grid by one step.  Returns true if there were any changes, false if not.
			/// </summary>
			public bool Iterate()
			{
				var next = new char[Width, Height];
				bool changed = false;

				foreach (var y in Enumerable.Range(0, Height))
					foreach (var x in Enumerable.Range(0, Width))
					{
						var oa = CountOccupiedFrom(x,y);
						var c = grid[x,y];
						switch (c)
						{
							case 'L' when oa == 0:         next[x,y] = '#'; changed = true; break;
							case '#' when oa >= Tolerance: next[x,y] = 'L'; changed = true; break;

							default: next[x,y] = c; break;
						}
					}

				grid = next;

				return changed;
			}

			public int CountAllOccupied()
			{
				int o = 0;
				foreach (var c in grid)
					if (c == '#') o++;
				return o;
			}

			static readonly (int, int)[] adjacentOffsets = {
				(-1, -1),  ( 0, -1),  ( 1, -1),
				(-1,  0),             ( 1,  0),
				(-1,  1),  ( 0,  1),  ( 1,  1)
			};

			int CountOccupiedFrom(int x, int y)
			{
				int count = 0;
				foreach (var (ax, ay) in adjacentOffsets)
				{
					var (cx, cy) = (x, y);

					do
					{
						cx += ax;
						cy += ay;
						if (cx < 0 || cx >= Width || cy < 0 || cy >= Height)
							goto next;
					}
					while (FirstVisible && grid[cx, cy] == '.');

					if (grid[cx, cy] == '#') count++;

					next:;
				}
				return count;
			}

			public override string ToString()
			{
				StringBuilder sb = new ();
				foreach (var y in Enumerable.Range(0, Height))
				{
					foreach (var x in Enumerable.Range(0, Width))
						sb.Append(grid[x,y]);
					sb.Append('\n');
				}
				return sb.ToString();						
			}

			private char[,] grid;

			private SeatingArea(int width, int height, int part)
			{
				Width = width;
				Height = height;

				grid = new char[width, height];

				switch (part)
				{
					case 1:
						Tolerance = 4;
						FirstVisible = false;
						break;

					case 2:
						Tolerance = 5;
						FirstVisible = true;
						break;

					default:
						throw new Exception($"Part ({part}) must be either 1 or 2.");
				}
			}
		}

		static int CountStableOccupied(string input, int part)
		{
			var sa = SeatingArea.Parse(input, part);

			while (sa.Iterate())
				; // intentional nop

			return sa.CountAllOccupied();
		}

		public override object Part1(string input)
		{
			return CountStableOccupied(input, 1);
		}

		public override object Part2(string input)
		{
			return CountStableOccupied(input, 2);
		}
	}
}
