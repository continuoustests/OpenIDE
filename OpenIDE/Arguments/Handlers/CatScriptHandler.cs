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
	class CatScriptHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Prints the script to the terminal");
					usage.Add("SCRIPT-NAME", "Script name with optional file extension.");
				return usage;
			}
		}

		public string Command { get { return "cat"; } }

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
			Console.WriteLine(File.ReadAllText(script.Location));
		}
	}
}