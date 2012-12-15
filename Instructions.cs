using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrainBreak
{
	public enum Instructions
	{
		IncrementAddress = '>',
		DecrementAddress = '<',
		IncrementValue = '+',
		DecrementValue = '-',
		Output = '.',
		Input = ',',
		BeginLoop = '[',
		EndLoop = ']',

		BitwiseOrOperator = '|',
		BitwiseAndOperator = '&',
		BitwiseNotOperator = '~',
		BitwiseXorOperator = '^',
		MultiplyValue = '*',
		DivideValue = '/',
		PrevAssignment = '(',
		NextAssignment = ')',
	}
}
