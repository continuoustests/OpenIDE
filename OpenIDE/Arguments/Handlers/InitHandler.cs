using System;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class InitHandler : ICommandHandler
	{
		private ConfigurationHandler _configHandler;

		public InitHandler(ConfigurationHandler configHandler) {
			_configHandler = configHandler;
		}

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Initializes and sets up a configuration point for OpenIDE");
				usage.Add("[LANGUAGES]", "Enabled languages for this config point using the first language as default language (C#,python)");
				usage.Add("[all]", "Initialize with all languages");
				return usage;
			}
		}

		public string Command { get { return "init"; } }
		
		public void Execute (string[] arguments)
		{
			try {
				_configHandler.Execute(new[] { "init" });
				if (arguments.Length == 0) {
					_configHandler.Execute(new[] { "enabled.languages=" });
					return;
				}
				if (arguments[0] == "all")
					return;

				var languages = "";
				var firstLanguage = "";
				foreach (var lang in arguments[0].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)) {
					if (languages == "") {
						languages = lang;
						firstLanguage = lang;
					} else
						languages += "," + lang;
				}
				_configHandler.Execute(new[] { "enabled.languages=" + languages });
				_configHandler.Execute(new[] { "default.language=" + firstLanguage });
			} catch {
				Console.WriteLine("Invalid command arguments");
			}
		}
	}
}