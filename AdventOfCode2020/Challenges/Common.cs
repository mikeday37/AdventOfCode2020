using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges
{
	public static class Common
	{
		public static IEnumerable<string> ToLines(this string input, bool trimEnd = true, bool omitBlanks = true)
		{
			foreach (var line in input.Split('\n'))
			{
				if (omitBlanks && string.IsNullOrWhiteSpace(line))
					continue;

				if (trimEnd)
					yield return line.TrimEnd();
				else
					yield return line;
			}
		}

		public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> e)
		{
			return e.Select((x,i) => (x,i));
		}
	}
}
