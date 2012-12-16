using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BrainBreak.Interpreter
{
	public class BrainBreakInterpreter
	{
		public BrainBreakInterpreter(string file, int memorySize = 1024)
		{
			if (memorySize <= 0) memorySize = 1024;
			_memorySize = memorySize;
			_memory = new int[memorySize];

			try
			{
				_program = File.ReadAllText(file);
			}
			catch (Exception e)
			{
				_program = "";
				Console.WriteLine(e.Message);
				return;
			}

			_instructions = new Dictionary<char, Action>();
			_instructions.Add((char)Instructions.IncrementValue, IncrementValue);
			_instructions.Add((char)Instructions.DecrementValue, DecrementValue);
			_instructions.Add((char)Instructions.IncrementAddress, IncrementAddress);
			_instructions.Add((char)Instructions.DecrementAddress, DecrementAddress);
			_instructions.Add((char)Instructions.BeginLoop, BeginLoop);
			_instructions.Add((char)Instructions.EndLoop, EndLoop);
			_instructions.Add((char)Instructions.Input, Input);
			_instructions.Add((char)Instructions.Output, Output);
			_instructions.Add((char)Instructions.BitwiseOrOperator, BitwiseOrOperator);
			_instructions.Add((char)Instructions.BitwiseAndOperator, BitwiseAndOperator);
			_instructions.Add((char)Instructions.BitwiseNotOperator, BitwiseNotOperator);
			_instructions.Add((char)Instructions.BitwiseXorOperator, BitwiseXorOperator);
			_instructions.Add((char)Instructions.MultiplyValue, MultiplyValue);
			_instructions.Add((char)Instructions.DivideValue, DivideValue);
			_instructions.Add((char)Instructions.PrevAssignment, PrevAssignment);
			_instructions.Add((char)Instructions.NextAssignment, NextAssignment);
			_nest = new Stack<int>();
		}

		public void Run()
		{
			char[] program = _program.Where(c => _instructions.ContainsKey(c)).ToArray();
			while (InstructionPointer < program.Length)
			{
				_instructions[program[InstructionPointer]]();
			}
		}

		#region Instructions

		public void IncrementValue()
		{
			_memory[_pointer]++;
			IncrementInstructionPointer();
		}

		public void DecrementValue()
		{
			_memory[_pointer]--;
			IncrementInstructionPointer();
		}

		public void IncrementAddress()
		{
			if (_pointer < MemorySize - 1)
				_pointer++;
			IncrementInstructionPointer();
		}

		public void DecrementAddress()
		{
			if (_pointer > 0)
				_pointer--;
			IncrementInstructionPointer();
		}

		public void BeginLoop()
		{
			_nest.Push(InstructionPointer);
			IncrementInstructionPointer();
		}

		public void EndLoop()
		{
			if (_memory[_pointer] <= 0)
			{
				_nest.Pop();
				IncrementInstructionPointer();
			}
			else
			{
				InstructionPointer = _nest.Pop();
			}
		}

		public void Input()
		{
			_memory[_pointer] = Console.Read();
			IncrementInstructionPointer();
		}

		public void Output()
		{
			Console.Write((char)_memory[_pointer]);
			IncrementInstructionPointer();
		}

		public void BitwiseOrOperator()
		{
			_memory[_pointer + 2] = _memory[_pointer] | _memory[_pointer + 1];
			IncrementAddress();
			IncrementInstructionPointer();
		}

		public void BitwiseAndOperator()
		{
			_memory[_pointer + 2] = _memory[_pointer] & _memory[_pointer + 1];
			IncrementAddress();
			IncrementInstructionPointer();
		}

		public void BitwiseNotOperator()
		{
			_memory[_pointer] = ~_memory[_pointer];
			IncrementAddress();
			IncrementInstructionPointer();
		}

		public void BitwiseXorOperator()
		{
			_memory[_pointer + 2] = _memory[_pointer] ^ _memory[_pointer + 1];
			IncrementAddress();
			IncrementInstructionPointer();
		}

		public void MultiplyValue()
		{
			_memory[_pointer] *= 2;
			IncrementInstructionPointer();
		}

		public void DivideValue()
		{
			_memory[_pointer] /= 2;
			IncrementInstructionPointer();
		}

		public void PrevAssignment()
		{
			if (_pointer > 0)
				_memory[_pointer - 1] = _memory[_pointer];
			IncrementInstructionPointer();
		}

		public void NextAssignment()
		{
			if (_pointer < MemorySize - 1)
				_memory[_pointer + 1] = _memory[_pointer];
			IncrementInstructionPointer();
		}

		public void IncrementInstructionPointer()
		{
			_instructionPointer++;
		}

		#endregion

		private Dictionary<char, Action> _instructions;
		private Stack<int> _nest;

		#region Properties

		private int _instructionPointer;
		public int InstructionPointer
		{
			get { return _instructionPointer; }
			private set { _instructionPointer = value; }
		}

		private int[] _memory;
		public int[] Memory
		{
			get { return _memory; }
			private set { _memory = value; }
		}

		private int _memorySize;
		public int MemorySize
		{
			get { return _memorySize; }
			private set { _memorySize = value; }
		}

		private string _program;
		public string Program
		{
			get { return _program; }
			private set { _program = value; }
		}

		private int _pointer;
		public int Pointer
		{
			get { return _pointer; }
			private set { _pointer = value; }
		}

		#endregion
	}
}
