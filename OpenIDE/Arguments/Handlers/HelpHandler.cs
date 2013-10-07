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
			var definitions = Bootstrapper.GetDefinitionBuilder().Definitions;
			if (arguments.Length != 1) {
				UsagePrinter.PrintDefinitionsAligned(
					definitions
						.OrderBy(x => x.Name));
				return;
			}
			var commandName = arguments[0];
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