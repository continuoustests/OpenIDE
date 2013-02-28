using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class HandleScriptHandler : ICommandHandler
	{
		private Action<string> _dispatch;
		private List<ICommandHandler> _handlers = new List<ICommandHandler>();

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"No arguments will list available scripts. Run scripts with 'oi x [script-name]'.");
					usage
						.Add("new", "Creates a new script")
							.Add("SCRIPT-NAME", "Script name with optional file extension.")
								.Add("[--global]", "Will create the new script in the main script folder")
									.Add("[-g]", "Short for --global");
					usage
						.Add("edit", "Opens a script for edit")
							.Add("SCRIPT-NAME", "Script name. Local are picked over global");
					usage
						.Add("rm", "Delete a script")
							.Add("SCRIPT-NAME", "Script name local are picked over global");
				return usage;
			}
		}

		public string Command { get { return "script"; } }

		public HandleScriptHandler(Action<string> dispatch)
		{
			_dispatch = dispatch;
			_handlers.Add(new ScriptHandler(_dispatch));
			_handlers.Add(new CreateScriptHandler(_dispatch));
			_handlers.Add(new EditScriptHandler(_dispatch));
			_handlers.Add(new DeleteScriptHandler());
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length == 0)
				arguments = new[] { "x" };
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