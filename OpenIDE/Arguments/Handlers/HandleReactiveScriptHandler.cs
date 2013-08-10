using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.RScripts;

namespace OpenIDE.Arguments.Handlers
{
	class HandleReactiveScriptHandler : ICommandHandler
	{
		private string _token;
		private Action<string> _dispatch;
		private PluginLocator _locator;
		private List<ICommandHandler> _handlers = new List<ICommandHandler>();

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Handles reactive scripts. No arguments will list available scripts");
				
				var newHandler = usage.Add("new", "Creates a script that is triggered by it's specified events");
				var newName = newHandler.Add("SCRIPT-NAME", "Script name with optional file extension.");
				newName.Add("[--global]", "Will create the new script in the main script folder")
					.Add("[-g]", "Short for --global");

				usage
					.Add("edit", "Opens an existing reactive script for editor")
					.Add("SCRIPT-NAME", "Reactive script name. Local are picked over global");

				usage
					.Add("rm", "Deletes a reactive script")
					.Add("SCRIPT-NAME", "Reactive script name. Local are picked over global");
				return usage;
			}
		}
	
		public string Command { get { return "rscript"; } }

		public HandleReactiveScriptHandler(string token, Action<string> dispatch, PluginLocator locator)
		{
			_token = token;
			_dispatch = dispatch;
			_locator = locator;
			_handlers.Add(new ListReactiveScriptsHandler(_token, _locator));
			_handlers.Add(new CreateReactiveScriptHandler(_token, _dispatch));
			_handlers.Add(new EditReactiveScriptHandler(_dispatch, _locator, _token));
			_handlers.Add(new DeleteReactiveScriptHandler(_locator, _token));
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length == 0)
				arguments = new[] { "list" };
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
	}
}