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
			args = "-f hello.bb".Split(' ');
			var options = new HashSet<string> { "-c", "-f", "-m", "-o" };
			string key = "";
			var res = args
				.GroupBy(s => options.Contains(s) ? key = s : key)
				.ToDictionary(g => g.Key, g => g.Skip(1).FirstOrDefault());

			var memory = res.ContainsKey("-m") ? int.Parse(res["-m"]) : 0;
			var file = res.ContainsKey("-f") ? res["-f"] : null;

			if (file != null)
			{
				if (res.ContainsKey("-c"))
				{
					var compiler = new BrainBreakCompiler();
				}
				else
				{
					var interpreter = new BrainBreakInterpreter(file, memory);
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
