using AdventOfCodeScaffolding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2020.Challenges
{
	[Challenge(4, "Passport Processing")]
	class Day4 : ChallengeBase
	{
		public override object Part1(string input)
		{
			var lines = input
				.Split('\n')
				.Select(x => x.Trim())
				.ToList();

			var parts = "byr,iyr,eyr,hgt,hcl,ecl,pid".Split(',');
			var build = "";
			int count = 0;

			foreach (var line in lines)
			{
				if (string.IsNullOrWhiteSpace(line))
				{
					if (string.IsNullOrWhiteSpace(build))
						continue;

					if (7 == parts.Count(x => build.Contains(x + ':')))
						count++;

					build = "";
				}
				else
					build += line + " ";
			}

			if (!string.IsNullOrWhiteSpace(build)
					&& 7 == parts.Count(x => build.Contains(x + ':')))
				count++;

			return count;
		}

		public override object Part2(string input)
		{
			var lines = input
				.Split('\n')
				.Select(x => x.Trim())
				.ToList();

			var build = "";
			int count = 0;

			foreach (var line in lines)
			{
				if (string.IsNullOrWhiteSpace(line))
				{
					if (string.IsNullOrWhiteSpace(build))
						continue;

					if (Validate(build))
						count++;

					build = "";
				}
				else
					build += line + " ";
			}


			if (Validate(build))
				count++;

			return count;
		}

		bool Validate(string block)
		{
			var parts = block
				.Split(' ')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => x.Split(':'));

			var check = "byr,iyr,eyr,hgt,hcl,ecl,pid"
				.Split(',')
				.ToDictionary(x => x, x => false);

			foreach (var part in parts)
			{
				var (key, value) = (part[0], part[1]);
				switch (key)
				{
					case "byr": check[key] = FourDigits(value, 1920, 2002); break;
					case "iyr": check[key] = FourDigits(value, 2010, 2020); break;
					case "eyr": check[key] = FourDigits(value, 2020, 2030); break;

					case "hgt":
					{
						int v;
						if (!int.TryParse(value.Substring(0, value.Length - 2), out v))
							return false;

						if (value.EndsWith("cm"))
							check[key] = 150 <= v && v <= 193;
						else if (value.EndsWith("in"))
							check[key] = 59 <= v && v <= 76;
						else
							return false;

						break;
					}

					case "hcl":
						check[key] =
							value.Length == 7
							&& value[0] == '#'
							&& value.Skip(1).All(x => "0123456789abcdef".Contains(x));
						break;

					case "ecl":
						check[key] = value.Length == 3 && "amb,blu,brn,gry,grn,hzl,oth,".Contains(value + ',');
						break;

					case "pid":
						check[key] = value.Length == 9 && value.All(x => char.IsDigit(x));
						break;

					case "cid": continue;

					default: throw new Exception("Unexpected field.");
				}
			}

			return check.Values.All(x => x);
		}

		private bool FourDigits(string s, int v1, int v2)
		{
			if (s.Length != 4 || !s.All(x => char.IsDigit(x)))
				return false;

			var v = int.Parse(s);
			
			return v1 <= v && v <= v2;
		}
	}
}

