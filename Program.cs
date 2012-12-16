using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrainBreak.Compiler;
using BrainBreak.Interpreter;

namespace BrainBreak
{
	public class Program
	{
		static void Main(string[] args)
		{
			args = "-c -f hello.bb -s".Split(' ');
			var options = new HashSet<string> { "-c", "-f", "-m", "-o", "-s" };
			string key = "";
			var res = args
				.GroupBy(s => options.Contains(s) ? key = s : key)
				.ToDictionary(g => g.Key, g => g.Skip(1).FirstOrDefault());

			var memory = res.ContainsKey("-m") ? int.Parse(res["-m"]) : 0;
			var inputfile = res.ContainsKey("-f") ? res["-f"] : null;
			var outputfile = res.ContainsKey("-o") ? res["-o"] : null;

			if (inputfile != null)
			{
				if (res.ContainsKey("-c"))
				{
					var compiler = new BrainBreakCompiler(inputfile, outputfile, memory);
					compiler.Compile(res.ContainsKey("-s"));
				}
				else
				{
					var interpreter = new BrainBreakInterpreter(inputfile, memory);
					interpreter.Run();
				}
			}
			else
			{
				Console.WriteLine("File not found");
			}
		}
	}
}
