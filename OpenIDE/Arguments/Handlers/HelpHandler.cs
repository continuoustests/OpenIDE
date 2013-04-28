using System;
using System.Linq;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Language;
using OpenIDE.Core.Commands;

namespace OpenIDE.Arguments.Handlers
{
	public class HelpHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Displays usage info for specified command(s)");

				usage.Add("COMMAND", "Command name");
				return usage;
			}
		}

		public string Command { get { return "help"; } }

		public void Execute(string[] arguments) {
			if (arguments.Length != 1) {
				Console.WriteLine("Invalid arguments");
				return;
			}
			var commandName = arguments[0];
			var definitions = Bootstrapper.GetDefinitionBuilder().Definitions;
			var command = definitions.FirstOrDefault(x => x.Name == commandName);
			if (command == null) {
				Console.WriteLine("{0} is not a valid command", commandName);
				return;
			}

			UsagePrinter.PrintDefinition(command);
			Console.WriteLine();
		}
	}
}