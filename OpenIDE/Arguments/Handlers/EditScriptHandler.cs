using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Definitions;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class EditScriptHandler : ICommandHandler
	{
		private Action<string> _dispatch;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Opens a script for edit");
					usage.Add("SCRIPT-NAME", "Script name. Local are picked over global");
					return usage;
			}
		}
	
		public string Command { get { return "edit"; } }
	
		public EditScriptHandler(Action<string> dispatch)
		{
			_dispatch = dispatch;
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length < 1)
				return;
			var scriptName = arguments[0];
			var script =
				Bootstrapper.GetDefinitionBuilder()
					.Definitions
					.FirstOrDefault(x => x.Name == scriptName &&
										 (x.Type == DefinitionCacheItemType.Script ||
										  x.Type == DefinitionCacheItemType.LanguageScript));
			if (script == null)
				return;
			_dispatch(string.Format("command|editor goto \"{0}|0|0\"", script.Location));
		}
	}
}
