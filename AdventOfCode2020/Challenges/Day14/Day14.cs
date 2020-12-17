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

namespace AdventOfCode2020.Challenges.Day14
{
	[Challenge(14, "Docking Data")]
	class Day14Challenge : ChallengeBase
	{
		private enum InstructionType
		{
			SetMask = 1,
			Write = 2
		}

		private record Instruction
		{	
			public InstructionType Type {get; init;}
			public string Mask {get; init;}
			public ulong Address {get; init;}
			public ulong Value {get; init;}

			public Instruction()
			{
				Mask = "";
				Address = 0;
				Value = 0;
			}
		}

		private IEnumerable<Instruction> ParseProgram(string input)
		{
			foreach (var line in input.ToLines())
			{
				switch (line[1])
				{
					case 'a':
						yield return new Instruction{
							Type = InstructionType.SetMask,
							Mask = line[7..]
						};
						break;

					case 'e':
						yield return new Instruction{
							Type = InstructionType.Write,
							Address = ulong.Parse(line[4..].Split(']').First()),
							Value = ulong.Parse(line.Split("= ").Last())
						};
						break;

					default:
						throw new Exception("Unexpected program line: " + line);
				}
			}
		}

		public override object Part1(string input)
		{
			Dictionary<ulong, ulong> memory = new();
			ulong andMask = 0, orMask = 0;
			foreach (var i in ParseProgram(input))
				switch (i.Type)
				{
					case InstructionType.SetMask:
						andMask = Convert.ToUInt64(i.Mask.Replace('X', '1'), 2);
						orMask = Convert.ToUInt64(i.Mask.Replace('X', '0'), 2);
						break;

					case InstructionType.Write:
						memory[i.Address] = (i.Value & andMask) | orMask;
						break;

					default:
						throw new Exception("Invalid instruction: " + i.ToString());
				}
			return memory.Values.Sum(x => (decimal)x);
		}

		public override object Part2(string input)
		{
			Dictionary<ulong, ulong> memory = new();
			string mask = "";
			ulong andMask = 0, orMask = 0;
			foreach (var i in ParseProgram(input))
				switch (i.Type)
				{
					case InstructionType.SetMask:
						mask = i.Mask;
						andMask = Convert.ToUInt64(mask.Replace('0', '1').Replace('X', '0'), 2);
						orMask = Convert.ToUInt64(mask.Replace('X', '0'), 2);
						break;

					case InstructionType.Write:
						var baseAddress = (i.Address & andMask) | orMask;
						ApplyFloatingBitWrite(memory, baseAddress, i.Value, mask);
						break;

					default:
						throw new Exception("Invalid instruction: " + i.ToString());
				}
			return memory.Values.Sum(x => (decimal)x);
		}

		private static void ApplyFloatingBitWrite(Dictionary<ulong, ulong> memory, ulong baseAddress, ulong value, string mask)
		{
			var xCount = mask.ToCharArray().Count(x => x == 'X');
			var writeCount = 1UL << xCount;
			for (var floatingBits = 0UL; floatingBits < writeCount; floatingBits++)
			{
				var address = baseAddress;
				ulong sourceBitValue = 1, destBitValue = 1;
				foreach (var m in mask.TrimStart('0').Reverse())
				{
					var bitValue = floatingBits & sourceBitValue;
					if (m == 'X')
					{
						if (bitValue != 0)
							address |= destBitValue;
						sourceBitValue <<= 1;
					}
					destBitValue <<= 1;
				}
				memory[address] = value;
			}
		}
	}
}
