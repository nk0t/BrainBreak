using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace BrainBreak.Compiler
{
	public class BBCodeGenerator
	{
		public BBCodeGenerator(string programName, string program,int memorySize)
		{
			_programName = programName;
			_program = program;
			_memorySize = memorySize;
		}

		public void Generate(string output)
		{
			CodeCompileUnit compileUnit = new CodeCompileUnit();
			CodeNamespace name = new CodeNamespace("BrainBreak");
			CodeTypeDeclaration mainClass = new CodeTypeDeclaration(_programName);
			CodeEntryPointMethod entryPoint = new CodeEntryPointMethod();
			mainClass.Members.Add(entryPoint);
			name.Types.Add(mainClass);
			name.Imports.Add(new CodeNamespaceImport("System"));
			compileUnit.Namespaces.Add(name);

			Initialize(entryPoint.Statements);

			var stmt_memorySize = new CodeVariableDeclarationStatement("System.Int32", "memorySize", new CodePrimitiveExpression(_memorySize));
			entryPoint.Statements.Add(stmt_memorySize);
			var stmt_memory = new CodeVariableDeclarationStatement("System.Int32[]", "memory", new CodeArrayCreateExpression("System.Int32[]", _memorySize));
			entryPoint.Statements.Add(stmt_memory);
			var stmt_pointer = new CodeVariableDeclarationStatement("System.Int32", "pointer", new CodePrimitiveExpression(0));
			entryPoint.Statements.Add(stmt_pointer);

			char[] program = _program.Where(c => _instructions.ContainsKey(c)).ToArray();
			while (InstructionPointer < program.Length)
			{
				_instructions[program[InstructionPointer]]();
			}

			CSharpCodeProvider provider = new CSharpCodeProvider();
			CodeGeneratorOptions options = new CodeGeneratorOptions();
			provider.GenerateCodeFromCompileUnit(compileUnit, Console.Out, options);
			CompilerParameters param = new CompilerParameters();

			param.GenerateExecutable = true;
			param.OutputAssembly = output;

			provider.CompileAssemblyFromDom(param, compileUnit);
		}

		private void Initialize(CodeStatementCollection entry_statements)
		{
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

			_expr_type_console = new CodeTypeReferenceExpression("Console");
			_expr_value_zero = new CodePrimitiveExpression(0);
			_expr_value_one = new CodePrimitiveExpression(1);
			_expr_value_two = new CodePrimitiveExpression(2);
			_expr_memory = new CodeVariableReferenceExpression("memory");
			_expr_pointer = new CodeVariableReferenceExpression("pointer");
			_expr_memory_pointer = new CodeArrayIndexerExpression(_expr_memory, _expr_pointer);
			_expr_memory_pointer_1 = new CodeArrayIndexerExpression(_expr_memory,
				new CodeBinaryOperatorExpression(_expr_pointer, CodeBinaryOperatorType.Add, _expr_value_one));
			_expr_memory_pointer_2 = new CodeArrayIndexerExpression(_expr_memory,
				new CodeBinaryOperatorExpression(_expr_pointer, CodeBinaryOperatorType.Add, _expr_value_two));
			_expr_memory_pointer_m1 = new CodeArrayIndexerExpression(_expr_memory,
				new CodeBinaryOperatorExpression(_expr_pointer, CodeBinaryOperatorType.Subtract, _expr_value_one));

			_nest = new Stack<CodeStatementCollection>();
			_nest.Push(entry_statements);
		}

		private CodeExpression _expr_type_console;
		private CodeExpression _expr_value_zero;
		private CodeExpression _expr_value_one;
		private CodeExpression _expr_value_two;
		private CodeExpression _expr_memory;
		private CodeExpression _expr_pointer;
		private CodeExpression _expr_memory_pointer_m1;
		private CodeExpression _expr_memory_pointer;
		private CodeExpression _expr_memory_pointer_1;
		private CodeExpression _expr_memory_pointer_2;
		private Stack<CodeStatementCollection> _nest;

		#region Instructions

		public void IncrementValue()
		{
			var stmt_increment_value =
				new CodeAssignStatement(_expr_memory_pointer,
					new CodeBinaryOperatorExpression(_expr_memory_pointer, CodeBinaryOperatorType.Add, _expr_value_one));
			_nest.Peek().Add(stmt_increment_value);

			IncrementInstructionPointer();
		}

		public void DecrementValue()
		{
			var stmt_decrement_value =
				new CodeAssignStatement(_expr_memory_pointer,
					new CodeBinaryOperatorExpression(_expr_memory_pointer, CodeBinaryOperatorType.Subtract, _expr_value_one));
			_nest.Peek().Add(stmt_decrement_value);

			IncrementInstructionPointer();
		}

		public void IncrementAddress()
		{
			var stmt_increment_pointer =
				new CodeAssignStatement(_expr_pointer,
					new CodeBinaryOperatorExpression(_expr_pointer, CodeBinaryOperatorType.Add, _expr_value_one));
			_nest.Peek().Add(stmt_increment_pointer);

			IncrementInstructionPointer();
		}

		public void DecrementAddress()
		{
			var stmt_swcrement_pointer =
				new CodeAssignStatement(_expr_pointer,
					new CodeBinaryOperatorExpression(_expr_pointer, CodeBinaryOperatorType.Subtract, _expr_value_one));
			_nest.Peek().Add(stmt_swcrement_pointer);

			IncrementInstructionPointer();
		}

		public void BeginLoop()
		{
			var cond = new CodeBinaryOperatorExpression(_expr_memory_pointer, CodeBinaryOperatorType.IdentityInequality, _expr_value_zero);
			var loop = new CodeIterationStatement(new CodeSnippetStatement(), cond, new CodeSnippetStatement());
			_nest.Peek().Add(loop);
			_nest.Push(loop.Statements);

			IncrementInstructionPointer();
		}

		public void EndLoop()
		{
			_nest.Pop();

			IncrementInstructionPointer();
		}

		public void Input()
		{
			var console_read = new CodeMethodInvokeExpression(_expr_type_console, "Read");
			_nest.Peek().Add(console_read);

			IncrementInstructionPointer();
		}

		public void Output()
		{
			var console_read = new CodeMethodInvokeExpression(_expr_type_console, "Write", new CodeCastExpression("System.char", _expr_memory_pointer));
			_nest.Peek().Add(console_read);

			IncrementInstructionPointer();
		}

		public void BitwiseOrOperator()
		{
			var stmt_bitwise_or =
				new CodeAssignStatement(_expr_memory_pointer_2,
					new CodeBinaryOperatorExpression(_expr_memory_pointer, CodeBinaryOperatorType.BitwiseOr, _expr_memory_pointer_1));
			_nest.Peek().Add(stmt_bitwise_or);

			IncrementAddress();

			IncrementInstructionPointer();
		}

		public void BitwiseAndOperator()
		{
			var stmt_bitwise_and =
				new CodeAssignStatement(_expr_memory_pointer_2,
					new CodeBinaryOperatorExpression(_expr_memory_pointer, CodeBinaryOperatorType.BitwiseAnd, _expr_memory_pointer_1));
			_nest.Peek().Add(stmt_bitwise_and);

			IncrementInstructionPointer();
		}

		public void BitwiseNotOperator()
		{
			/* memory[pointer] = ~memory[pointer];
			pointer++;*/

			/* いつか実装する */

			IncrementInstructionPointer();
		}

		public void BitwiseXorOperator()
		{
			/* memory[pointer + 2] = memory[pointer] ^ memory[pointer + 1];
			pointer++;*/

			/* いつか実装する */

			IncrementInstructionPointer();
		}

		public void MultiplyValue()
		{
			var stmt_multiply_value =
				new CodeAssignStatement(_expr_memory_pointer,
					new CodeBinaryOperatorExpression(_expr_memory_pointer, CodeBinaryOperatorType.Multiply, _expr_value_two));
			_nest.Peek().Add(stmt_multiply_value);

			IncrementInstructionPointer();
		}

		public void DivideValue()
		{
			var stmt_divide_value =
				new CodeAssignStatement(_expr_memory_pointer,
					new CodeBinaryOperatorExpression(_expr_memory_pointer, CodeBinaryOperatorType.Divide, _expr_value_two));
			_nest.Peek().Add(stmt_divide_value);

			IncrementInstructionPointer();
		}

		public void PrevAssignment()
		{
			var stmt_prev_assignment =
				new CodeAssignStatement(_expr_memory_pointer_m1, _expr_memory_pointer);
			_nest.Peek().Add(stmt_prev_assignment);

			IncrementInstructionPointer();
		}

		public void NextAssignment()
		{
			var stmt_next_assignment =
				new CodeAssignStatement(_expr_memory_pointer_1, _expr_memory_pointer);
			_nest.Peek().Add(stmt_next_assignment);

			IncrementInstructionPointer();
		}

		public void IncrementInstructionPointer()
		{
			_instructionPointer++;
		}

		#endregion

		private Dictionary<char, Action> _instructions;

		private int _instructionPointer;
		public int InstructionPointer
		{
			get { return _instructionPointer; }
			private set { _instructionPointer = value; }
		}

		private string _programName;
		public string ProgramName
		{
			get { return _programName; }
			set { _programName = value; }
		}

		private string _program;
		public string Program
		{
			get { return _program; }
			set { _program = value; }
		}

		private int _memorySize;
		public int MemorySize
		{
			get { return _memorySize; }
			set { _memorySize = value; }
		}
	}
}
