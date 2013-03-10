using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Bootstrapping;

namespace OpenIDE.Arguments.Handlers
{
	class GetCommandsHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Lists available commands for shell completion");
				return usage;
			}
		}

		public string Command { get { return "get-commands"; } }

		public void Execute(string[] arguments) {
			var args = new Stack<string>();
			for (int i = arguments.Length - 1; i >= 0; i--) 
				args.Push(arguments[i]);
			
			var usages = 
				Bootstrapper
					.GetCommandHandlers()
					.ToList();
			var item = pop(args);
			printOptions(item, args, usages);
		}

		private void printOptions(string lastItem, Stack<string> args, List<ICommandHandler> usages) {
			if (lastItem == null) {
				printAll(usages);
				return;
			}
			var parameters =
				usages
					.Where(x => x.Command.StartsWith(lastItem))
					.Select(x => x.Usage)
					.Cast<BaseCommandHandlerParameter>()
					.ToList();
			var defaultLang = 
				usages
					.FirstOrDefault(x => 
						x.Command == Bootstrapper.Settings.DefaultLanguage);
			if (defaultLang != null) {
				parameters.AddRange(defaultLang.Usage.Parameters);
				parameters.RemoveAll(x => x.Name == defaultLang.Command);
			}

			printOptions(
				lastItem,
				args,
				parameters);
		}

		private void printOptions(string lastItem, Stack<string> args, List<BaseCommandHandlerParameter> usages) {
			if (lastItem == null) {
				printAll(usages);
				return;
			}
			var item = pop(args);
			var match = usages.FirstOrDefault(x => x.Name == lastItem);
			if (item == null && match == null) {
				foreach (var usage in usages) {
					if (usage.Name.StartsWith(lastItem))
						Console.Write(usage.Name + " ");
				}
			} else {
				foreach (var usage in usages) {
					if (usage.Name == lastItem) {
						printOptions(item, args, usage.Parameters.ToList());
						break;
					}
				}
			}
		}

		private void printAll(List<ICommandHandler> handlers) {
			foreach (var handler in handlers) {
				if (handler.Command == Bootstrapper.Settings.DefaultLanguage)
					printAll(handler.Usage.Parameters.ToList());
				else
					Console.Write(handler.Command + " ");
			}
		}

		private void printAll(List<BaseCommandHandlerParameter> usages) {
			foreach (var usage in usages) {
				if (usage.Required && usage.Name != usage.Name.ToUpper())
					Console.Write(usage.Name + " ");
			}
		}

		private string pop(Stack<string> args) {
			if (args.Count > 0)
				return args.Pop();
			else
				return null;
		}
	}
}