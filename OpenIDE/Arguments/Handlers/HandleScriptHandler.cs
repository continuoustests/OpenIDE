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
		enum PrintType
		{
			Names,
			Simple,
			Full
		}

		private string _token;
		private Action<string> _dispatch;
		private List<ICommandHandler> _handlers = new List<ICommandHandler>();

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"No arguments will list available scripts.");
					usage.Add("[-l]", "Print script details when listing scripts");
					usage.Add("[-n]", "Will print only script names (overrides -l)");
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
					usage
						.Add("repl", "Continuously tests the specified script when saved")
                        	.Add("SCRIPT-NAME", "Script name")
                            	.Add("PARAMS", "Either script arguments or full command like: oi help mycommand");
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
			_handlers.Add(new TestScriptHandler(_dispatch, _token));
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length == 0 || arguments.Any(x => x == "-n")) {
				var type = arguments.Any(x => x == "-n") ? PrintType.Names : PrintType.Simple;
				printDefinitions(type);
				return;
			}
			if (arguments.Length == 0 || arguments.Any(x => x == "-l")) {
				var type = arguments.Any(x => x == "-l") ? PrintType.Full : PrintType.Simple;
				printDefinitions(type);
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

		private void printDefinitions(PrintType type) {
			var definitions = 
				Bootstrapper.GetDefinitionBuilder()
					.Definitions
					.Where(x => 
						x.Type == DefinitionCacheItemType.Script ||
						x.Type == DefinitionCacheItemType.LanguageScript);
			if (type == PrintType.Simple) {
				Console.WriteLine("Available commands:");
				UsagePrinter.PrintDefinitionsAligned(definitions);
			} else if (type == PrintType.Names) {
				UsagePrinter.PrintDefinitionNames(definitions);
			} else {
				Console.WriteLine("Available commands:");
				foreach (var definition in definitions)
					UsagePrinter.PrintDefinition(definition);
			}
		}
	}
}