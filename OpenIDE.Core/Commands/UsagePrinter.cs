using System;
using OpenIDE.Core.Language;

namespace OpenIDE.Core.Commands
{
	public class UsagePrinter
	{
		public static void PrintCommand(CommandHandlerParameter command)
		{
			Console.WriteLine("");
			Console.WriteLine("\t{2} : {0} ({1})",
				command.GetDescription(Environment.NewLine + "\t"),
				command.Language,
				command.Name);
		}

		public static void PrintParameter(BaseCommandHandlerParameter parameter, ref int level)
		{
			level++;
			var name = parameter.Name;
			if (!parameter.Required)
				name = "[" + name + "]";
			Console.WriteLine("{0}{1} : {2}", "".PadLeft(level, '\t'), name, parameter.Description);
			foreach (var child in parameter.Parameters)
				PrintParameter(child, ref level);
			level--;
		}
	}
}