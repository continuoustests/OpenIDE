using System;
using System.Linq;
using OpenIDE.Messaging;
using OpenIDE.Core.FileSystem;
using OpenIDE.Bootstrapping;
using OpenIDE.Arguments;
using OpenIDE.Core.Language;
using OpenIDE.CommandBuilding;
using OpenIDE.Core.CommandBuilding;
namespace oi
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Bootstrapper.Initialize();
			args = Bootstrapper.Settings.Parse(args);
			if (args.Length == 0)
			{
				printUsage(null);
				return;
			}
			var execute = Bootstrapper.GetDispatcher();
			var parser = new CommandStringParser();
			execute.For(
				parser.GetCommand(args),
				parser.GetArguments(args),
				(command) => printUsage(command));
		}

		private static void printUsage(string commandName)
		{
			if (commandName == null) {
				Console.WriteLine("OpenIDE.Net v0.1");
				Console.WriteLine("OpenIDE.Net is a cross language system that provides simple IDE features around your favorite text exitor.");
				Console.WriteLine("(http://www.openide.net, http://github.com/ContinuousTests/OpenIDE)");
				Console.WriteLine();
			}
			var level = 1;
			var handlers = Bootstrapper.GetCommandHandlers();
			var isHint = false;
			if (commandName != null) {
				handlers = handlers
					.Where(x => 
						 x.Command.Contains(commandName) ||
						(
							x.Usage != null && 
							x.Usage.Parameters.Any(y => y.Required && matchName(y.Name, commandName))
						));
				if (handlers.Count() > 0)
					Console.WriteLine("Did you mean:");
				isHint = true;
			}
			handlers.ToList()
				.ForEach(x =>
					{
						var command = x.Usage;
						if (command == null)
							return;

						if (command.Name != Bootstrapper.Settings.DefaultLanguage)
							printCommand(command);
						else
						{
							Console.WriteLine("");
							Console.WriteLine("\tDirect commands because of default language {0}", command.Name);
							level--;
						}

						command.Parameters.ToList()
							.ForEach(y =>  {
								if (commandName == null || y.Required && matchName(y.Name, commandName))
									printParameter(y, ref level);
							});

						if ((command.Name == Bootstrapper.Settings.DefaultLanguage))
							level++;
					});
			Console.WriteLine();
		}

		private static bool matchName(string actual, string parameter)
		{
			if (actual.Contains(parameter))
				return true;
			if (Math.Abs(actual.Length - parameter.Length) > 2)
				return false;
			var containedCharacters = 0;
			for (int i = 0; i < actual.Length; i++) {
				if (parameter.Contains(actual[i]))
					containedCharacters++;
			}
			if (containedCharacters > actual.Length - 2)
				return true;
			return false;
		}
		
		private static void printCommand(CommandHandlerParameter command)
		{
			Console.WriteLine("");
			Console.WriteLine("\t{2} : {0} ({1})",
				command.GetDescription(Environment.NewLine + "\t"),
				command.Language,
				command.Name);
		}

		private static void printParameter(BaseCommandHandlerParameter parameter, ref int level)
		{
			level++;
			var name = parameter.Name;
			if (!parameter.Required)
				name = "[" + name + "]";
			Console.WriteLine("{0}{1} : {2}", "".PadLeft(level, '\t'), name, parameter.Description);
			foreach (var child in parameter.Parameters)
				printParameter(child, ref level);
			level--;
		}
	}
}

