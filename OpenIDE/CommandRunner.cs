using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Definitions;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Language;
using OpenIDE.Core.Logging;
using OpenIDE.Core.CommandBuilding;

namespace OpenIDE
{
	public class CommandRunner
	{
		public bool Run(DefinitionCacheItem cmd, IEnumerable<string> args) {
			var arguments = args.ToList();
			arguments.RemoveAt(0);
			if (cmd.Type == DefinitionCacheItemType.Script || cmd.Type == DefinitionCacheItemType.LanguageScript) {
				Logger.Write("Running command as script");
				var script = new Script(Bootstrapper.Settings.RootPath, Environment.CurrentDirectory, cmd.Location);
				var sb = new StringBuilder();
				// On language commands remove the second argument too if
				// it matches the command name (oi langcommand vs oi C# langcommand)
				if (cmd.Type == DefinitionCacheItemType.LanguageScript && arguments.Count > 0 && arguments[0] == cmd.Name)
					arguments.RemoveAt(0);
				for (int i = 0; i < arguments.Count; i++) {
					sb.Append(" \"" + arguments[i] + "\"");
				}
				script.Run(
					sb.ToString(),
					(command) => {
						Bootstrapper.DispatchAndCompleteMessage(
							command,
							() => {
								Logger.Write("Writing end of command");
								script.Write("end-of-command");
							});
					});
			} else if (cmd.Type == DefinitionCacheItemType.Language) {
				Logger.Write("Running command as language command");
				var language = new LanguagePlugin(cmd.Location, Bootstrapper.DispatchMessage);
				// If default language command add original parameter
				if (args.ElementAt(0) != language.GetLanguage())
					arguments.Insert(0, args.ElementAt(0));
				language.Run(arguments.ToArray());
			} else {
				Logger.Write("Running command as built in command");
				var command = Bootstrapper.GetDefaultHandlers()
					.FirstOrDefault(x => x.Command == args.ElementAt(0));
				if (command == null)
					return false;
				command.Execute(arguments.ToArray());
			}
			// TODO: Command completed event needs to be triggered when command is done not when command process is started
			//Bootstrapper.DispatchMessage("event|command-completed " + new CommandStringParser().GetArgumentString(args));
			return true;
		}	
	}
}