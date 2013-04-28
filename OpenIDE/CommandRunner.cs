using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Definitions;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Language;

namespace OpenIDE
{
	public class CommandRunner
	{
		public bool Run(DefinitionCacheItem cmd, IEnumerable<string> args) {
			var arguments = args.ToList();
			arguments.RemoveAt(0);
			if (cmd.Type == DefinitionCacheItemType.Script) {
				var script = new Script(Bootstrapper.Settings.RootPath, Environment.CurrentDirectory, cmd.Location);
				var sb = new StringBuilder();
				for (int i = 0; i < arguments.Count; i++)
					sb.Append(" \"" + arguments[i] + "\"");
				script.Run(sb.ToString(), Bootstrapper.DispatchMessage);
			} else if (cmd.Type == DefinitionCacheItemType.Language) {
				var language = new LanguagePlugin(cmd.Location, Bootstrapper.DispatchMessage);
				// If default language command add original parameter
				if (args.ElementAt(0) != language.GetLanguage())
					arguments.Insert(0, args.ElementAt(0));
				language.Run(arguments.ToArray());
			} else {
				var command = Bootstrapper.GetDefaultHandlers()
					.FirstOrDefault(x => x.Command == args.ElementAt(0));
				if (command == null)
					return false;
				command.Execute(arguments.ToArray());
			}
			return true;
		}	
	}
}