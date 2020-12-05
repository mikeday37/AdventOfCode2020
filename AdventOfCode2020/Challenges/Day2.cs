using AdventOfCodeScaffolding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2020.Challenges
{
	
	[Challenge(2, "Password Philosophy")]
	class Day2 : ChallengeBase
	{
		public override object Part1(string input)
		{
			var lines = input
				.Split('\n')
				.Select(x => x.Trim())
				.Where(x => !string.IsNullOrWhiteSpace(x));
				
			int good = 0;

			foreach (var line in lines)
			{
				var info = ExtractLine(line);

				var count = info.password.ToCharArray().Count(x => x == info.c);
				if (count >= info.min && count <= info.max)
					good++;
			}

			return good;
		}

		private struct Info
		{
			public int min, max;
			public char c;
			public string password;
		}


		private Info ExtractLine(string line)
		{
			Info info = new Info();
			var a = line.Split(':');
			info.password = a[1].Trim();
			var b = a[0].Split(' ');
			info.c = b[1][0];
			var c = b[0].Trim().Split('-').Select(x => int.Parse(x)).ToArray();
			info.min = c[0];
			info.max = c[1];
			return info;
		}


		public override object Part2(string input)
		{
			var lines = input
				.Split('\n')
				.Select(x => x.Trim())
				.Where(x => !string.IsNullOrWhiteSpace(x));
				
			int good = 0;

			foreach (var line in lines)
			{
				var info = ExtractLine2(line);

				var matches = 0;

				if (info.c == info.password[info.a-1])
					matches++;

				if (info.c == info.password[info.b-1])
					matches++;

				if (matches == 1)
					good++;
			}

			return good;
		}

		private struct Info2
		{
			public int a, b;
			public char c;
			public string password;
		}


		private Info2 ExtractLine2(string line)
		{
			Info2 info = new Info2();
			var a = line.Split(':');
			info.password = a[1].Trim();
			var b = a[0].Split(' ');
			info.c = b[1][0];
			var c = b[0].Trim().Split('-').Select(x => int.Parse(x)).ToArray();
			info.a = c[0];
			info.b = c[1];
			return info;
		}
	}
}
