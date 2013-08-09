using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Definitions;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class DeleteScriptHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Delete a script");
					usage.Add("SCRIPT-NAME", "Script name local are picked over global");
					return usage;
			}
		}
	
		public string Command { get { return "rm"; } }

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
			if (File.Exists(script.Location))
				File.Delete(script.Location);
		}
	}
}
