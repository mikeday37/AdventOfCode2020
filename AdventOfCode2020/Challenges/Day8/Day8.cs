using AdventOfCodeScaffolding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges.Day8
{
	[Challenge(8, "Handheld Halting")]
	class Day8Challenge : ChallengeBase
	{
		public Instruction ParseInstruction(string x)
		{
			return new Instruction(
				x[0] switch {
					'n' => OpCode.Nop,
					'a' => OpCode.Acc,
					'j' => OpCode.Jmp,
					_ => throw new Exception("Unexpected instruction: {x}")
				},
				int.Parse(x[4..])
			);
		}

		public Instruction[] ParseProgram(string input)
		{
			return input
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => ParseInstruction(x))
				.ToArray();
		}

		public HaltInfo RunProgram(Instruction[] program)
		{
			Registers current = new(), prior = current;
			var visited = new BitArray(program.Length, false);
			HaltInfo halt(HaltReason reason) => new HaltInfo(reason, current, prior);
			
			for (; ;)
			{
				if (visited.Get(current.Index))
					return halt(HaltReason.InfiniteLoopDetected);

				prior = current;

				visited.Set(current.Index, true);

				var i = program[current.Index];
				switch (i.Op)
				{
					case OpCode.Acc:
						current.Index++;
						current.Accumulator += i.Arg;
						break;

					case OpCode.Nop:
						current.Index++;
						break;

					case OpCode.Jmp:
						current.Index += i.Arg; 
						if (current.Index < 0)
							return halt(HaltReason.JumpBeforeInstructions);
						if (current.Index > program.Length)
							return halt(HaltReason.JumpAfterInstructions);
						break;

					default: throw new InvalidOperationException();
				}

				if (current.Index == program.Length)
					return halt(HaltReason.EndOfInstructions);
			}
		}

		public override object Part1(string input)
		{
			var program = ParseProgram(input);
			var haltInfo = RunProgram(program);
			
			if (haltInfo.Reason == HaltReason.InfiniteLoopDetected)
				return haltInfo.Prior.Accumulator;
			else
				throw new Exception($"Unexpected halt: {haltInfo}");
		}

		public override object Part2(string input)
		{
			var program = ParseProgram(input);

			foreach (var suspect in Enumerable.Range(0, program.Length))
			{
				var original = program[suspect];

				switch (original.Op)
				{
					case OpCode.Acc: continue;
					case OpCode.Jmp: program[suspect] = original with {Op = OpCode.Nop}; break;
					case OpCode.Nop: program[suspect] = original with {Op = OpCode.Jmp}; break;
					default: throw new Exception($"Unexpected instruction: {original}");
				}

				var info = RunProgram(program);
				program[suspect] = original;

				if (info.Reason == HaltReason.EndOfInstructions)
					return info.Current.Accumulator;
			}

			throw new Exception("Did not find single-instruction fix.");
		}
	}
}
