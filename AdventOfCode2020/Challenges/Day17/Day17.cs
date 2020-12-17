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
		public class PocketDimension
		{
			private readonly HashSet<(int, int, int)> live = new();

			public int XMin { get; private set; }
			public int XMax { get; private set; }
			public int YMin { get; private set; }
			public int YMax { get; private set; }
			public int ZMin { get; private set; }
			public int ZMax { get; private set; }

			public void Add((int, int, int) c)
			{
				if (c.Item1 < XMin) XMin = c.Item1;
				if (c.Item1 > XMax) XMax = c.Item1;

				if (c.Item2 < YMin) YMin = c.Item2;
				if (c.Item2 > YMax) YMax = c.Item2;

				if (c.Item3 < ZMin) ZMin = c.Item3;
				if (c.Item3 > ZMax) ZMax = c.Item3;

				live.Add(c);
			}

			public bool Contains((int, int, int) c)
			{
				return live.Contains(c);
			}

			public IEnumerable<string> ToDisplayStrings()
			{
				for (int z = ZMin; z <= ZMax; z++)
				{
					StringBuilder sb = new();
					for (int y = YMin; y <= YMax; y++)
					{
						if (y > YMin) sb.Append('\n');
						for (int x = XMin; x <= XMax; x++)
							sb.Append(Contains((x, y, z)) ? '#' : '.');
					}
					yield return sb.ToString();
				}
			}

			public int LiveCellCount => live.Count;
		}

		void LogPocketDimension(string sectionHeader, PocketDimension pd)
		{
			using (Logger.Context(sectionHeader))
				foreach (var l in pd.ToDisplayStrings().WithIndex())
					using (Logger.Context($"z = {l.Item2 + pd.ZMin}"))
						Logger.LogLine(l.Item1);
		}

		static PocketDimension Iterate(PocketDimension prev)
		{
			PocketDimension next = new();

			for (int z = prev.ZMin - 1; z <= prev.ZMax + 1; z++)
				for (int y = prev.YMin - 1; y <= prev.YMax + 1; y++)
					for (int x = prev.XMin - 1; x <= prev.XMax + 1; x++)
					{
						int liveNeighbors = 0;
						
						for (int ox = -1; ox <= 1; ox++)
							for (int oy = -1; oy <= 1; oy++)
								for (int oz = -1; oz <= 1; oz++)
								{
									if (ox == 0 && oy == 0 && oz == 0)
										continue;

									if (prev.Contains((x + ox, y + oy, z + oz)))
										liveNeighbors++;
								}

						if (prev.Contains((x, y, z)))
						{
							if (liveNeighbors == 2 || liveNeighbors == 3)
								next.Add((x, y, z));
						}
						else
						{
							if (liveNeighbors == 3)
								next.Add((x, y, z));
						}
					}

			return next;
		}


		public override object Part1(string input)
		{
			PocketDimension pd = new();
			foreach (var (line, y) in input.ToLines().WithIndex())
				foreach (var (v, x) in line.WithIndex())
					if (v == '#')
						pd.Add((x, y, 0));

			LogPocketDimension("Initial State:", pd);

			foreach (var cycle in Enumerable.Range(1, 6))
			{
				pd = Iterate(pd);
				LogPocketDimension($"\nAfter {cycle} cycles:", pd);
			}

			return pd.LiveCellCount;
		}

		public class PocketDimension2
		{
			private readonly HashSet<(int, int, int, int)> live = new();

			public int XMin { get; private set; }
			public int XMax { get; private set; }
			public int YMin { get; private set; }
			public int YMax { get; private set; }
			public int ZMin { get; private set; }
			public int ZMax { get; private set; }
			public int QMin { get; private set; }
			public int QMax { get; private set; }

			public void Add((int, int, int, int) c)
			{
				if (c.Item1 < XMin) XMin = c.Item1;
				if (c.Item1 > XMax) XMax = c.Item1;

				if (c.Item2 < YMin) YMin = c.Item2;
				if (c.Item2 > YMax) YMax = c.Item2;

				if (c.Item3 < ZMin) ZMin = c.Item3;
				if (c.Item3 > ZMax) ZMax = c.Item3;

				if (c.Item4 < QMin) QMin = c.Item4;
				if (c.Item4 > QMax) QMax = c.Item4;

				live.Add(c);
			}

			public bool Contains((int, int, int, int) c)
			{
				return live.Contains(c);
			}

			public int LiveCellCount => live.Count;
		}

		static PocketDimension2 Iterate2(PocketDimension2 prev)
		{
			PocketDimension2 next = new();

			for (int q = prev.QMin - 1; q <= prev.QMax + 1; q++)
				for (int z = prev.ZMin - 1; z <= prev.ZMax + 1; z++)
					for (int y = prev.YMin - 1; y <= prev.YMax + 1; y++)
						for (int x = prev.XMin - 1; x <= prev.XMax + 1; x++)
						{
							int liveNeighbors = 0;
						
							for (int ox = -1; ox <= 1; ox++)
								for (int oy = -1; oy <= 1; oy++)
									for (int oz = -1; oz <= 1; oz++)
										for (int oq = -1; oq <= 1; oq++)
										{
											if (ox == 0 && oy == 0 && oz == 0 && oq == 0)
												continue;

											if (prev.Contains((x + ox, y + oy, z + oz, q + oq)))
												liveNeighbors++;
										}

							if (prev.Contains((x, y, z, q)))
							{
								if (liveNeighbors == 2 || liveNeighbors == 3)
									next.Add((x, y, z, q));
							}
							else
							{
								if (liveNeighbors == 3)
									next.Add((x, y, z, q));
							}
						}

			return next;
		}


		public override object Part2(string input)
		{
			PocketDimension2 pd = new();
			foreach (var (line, y) in input.ToLines().WithIndex())
				foreach (var (v, x) in line.WithIndex())
					if (v == '#')
						pd.Add((x, y, 0, 0));

			foreach (var cycle in Enumerable.Range(1, 6))
				pd = Iterate2(pd);

			return pd.LiveCellCount;
		}
	}
}
