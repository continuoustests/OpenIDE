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
			var builder = Bootstrapper.GetDefinitionBuilder(); 
			builder.Build(); 
			var args = new Stack<string>();
			for (int i = arguments.Length - 1; i >= 0; i--) 
				args.Push(arguments[i]);
			var item = pop(args);
			printOptions(item, args, builder.Definitions);
		}

		private void printOptions(string lastItem, Stack<string> args, IEnumerable<OpenIDE.Core.Definitions.DefinitionCacheItem> usages) {
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
						printOptions(item, args, usage.Parameters);
						break;
					}
				}
			}
		}

		private void printAll(IEnumerable<OpenIDE.Core.Definitions.DefinitionCacheItem> usages) {
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