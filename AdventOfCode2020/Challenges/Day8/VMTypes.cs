using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges.Day8
{
	public struct Registers
	{
		public int Index, Accumulator;

		public override string ToString() =>
			$"I: {Index}, A: {Accumulator}"; 
	}

	public enum OpCode {Nop, Acc, Jmp}

	public record Instruction
	{
		public OpCode Op {get; init;}
		public int Arg {get; init;}

		public Instruction(OpCode op, int arg) => (Op, Arg) = (op, arg);
	}

	public enum HaltReason
	{
		EndOfInstructions, // Normal termination

		JumpBeforeInstructions,
		JumpAfterInstructions,
		InfiniteLoopDetected
	}

	public record HaltInfo
	{
		public HaltReason Reason {get;}
		public Registers Current {get;}
		public Registers Prior {get;}

		public HaltInfo(HaltReason reason, Registers current, Registers prior) => (Reason, Current, Prior) = (reason, current, prior);

		public override string ToString() =>
			$"Reason = {Reason},  Current [{Current}],  Prior [{Prior}]"; 
	}
}
