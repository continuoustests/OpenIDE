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
			var handlers = Bootstrapper.GetCommandHandlers();
			var command = handlers.FirstOrDefault(x => x.Command == commandName);
			if (command == null) {
				Console.WriteLine("{0} is not a valid command", commandName);
				return;
			}
			if (command.Usage != null) {
				UsagePrinter.PrintCommand(command.Usage);

				var level = 1;
				command.Usage.Parameters.ToList()
					.ForEach(y =>  {
						UsagePrinter.PrintParameter(y, ref level);
					});
				Console.WriteLine();
			}
		}
	}
}