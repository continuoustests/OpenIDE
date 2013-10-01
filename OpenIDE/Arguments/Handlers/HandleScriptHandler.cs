using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Commands;
using OpenIDE.Core.Language;
using OpenIDE.Core.Definitions;

namespace OpenIDE.Arguments.Handlers
{
	class HandleScriptHandler : ICommandHandler
	{
		private string _token;
		private Action<string> _dispatch;
		private List<ICommandHandler> _handlers = new List<ICommandHandler>();

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"No arguments will list available scripts. Run scripts with 'oi x [script-name]'.");
					var name = usage
						.Add("new", "Creates a new script")
							.Add("SCRIPT-NAME", "Script name with optional file extension.");
					name.Add("[--global]", "Will create the new script in the main script folder")
						.Add("[-g]", "Short for --global");
					
					usage
						.Add("edit", "Opens a script for edit")
							.Add("SCRIPT-NAME", "Script name. Local are picked over global");
					usage
						.Add("rm", "Delete a script")
							.Add("SCRIPT-NAME", "Script name local are picked over global");
					usage
						.Add("cat", "Prints the script to the terminal")
							.Add("SCRIPT-NAME", "Script name with optional file extension.");
				return usage;
			}
		}

		public string Command { get { return "script"; } }

		public HandleScriptHandler(string token, Action<string> dispatch, Func<PluginLocator> pluginLocator)
		{
			_token = token;
			_dispatch = dispatch;
			_handlers.Add(new CreateScriptHandler(_token, _dispatch, pluginLocator));
			_handlers.Add(new EditScriptHandler(_dispatch));
			_handlers.Add(new DeleteScriptHandler());
			_handlers.Add(new CatScriptHandler());
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length == 0) {
				printDefinitions();
				return;
			}
			var handler = _handlers.FirstOrDefault(x => x.Command == arguments[0]);
			if (handler == null)
				return;
			handler.Execute(getArguments(arguments));
		}

		private string[] getArguments(string[] args)
		{
			var arguments = new List<string>();
			for (int i = 1; i < args.Length; i++)
				arguments.Add(args[i]);
			return arguments.ToArray();
		}

		private void printDefinitions() {
			var definitions = 
				Bootstrapper.GetDefinitionBuilder()
					.Definitions
					.Where(x => x.Type == DefinitionCacheItemType.Script);
			Console.WriteLine("Available commands:");
			foreach (var definition in definitions)
				UsagePrinter.PrintDefinition(definition);
		}
	}
}