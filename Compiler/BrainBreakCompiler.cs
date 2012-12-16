using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BrainBreak.Compiler
{
	public class BrainBreakCompiler
	{
		public BrainBreakCompiler(string file, string output = "", int memorySize = 1024)
		{
			if (memorySize <= 0) memorySize = 1024;
			_memorySize = memorySize;

			if (string.IsNullOrEmpty(output))
			{
				_output = Path.ChangeExtension(file, "exe");
			}
			else
			{
				_output = output;
			}
			_programName = Path.GetFileNameWithoutExtension(_output);

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
		}

		public void Compile(bool showCSCode = false)
		{
			var generator = new BBCodeGenerator(_programName, _program, _memorySize);
			generator.Generate(_output, showCSCode);
		}


		private string _programName;
		public string ProgramName
		{
			get { return _programName; }
			private set { _programName = value; }
		}

		private string _output;
		public string Output
		{
			get { return _output; }
			set { _output = value; }
		}

		private string _program;
		public string Program
		{
			get { return _program; }
			private set { _program = value; }
		}

		private int _memorySize;
		public int MemorySize
		{
			get { return _memorySize; }
			private set { _memorySize = value; }
		}
	}
}
