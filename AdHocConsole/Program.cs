using System;

namespace AdHocConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine(ParseSeatID("BFFFBBFRRR"));
			Console.WriteLine(ParseSeatID("FFFBBBFRRR"));
			Console.WriteLine(ParseSeatID("BBFFBBFRLL"));

			Console.ReadKey();
		}

		static int ParseSeatID(string bsp)
		{
			return Convert.ToInt32(
				bsp
					.Replace('F','0')
					.Replace('B','1')
					.Replace('L','0')
					.Replace('R','1')
				,
				2);
		}
	}
}
