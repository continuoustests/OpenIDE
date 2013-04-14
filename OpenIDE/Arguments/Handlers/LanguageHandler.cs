using System;
using System.Linq;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class LanguageHandler : ICommandHandler
	{
		private CommandHandlerParameter _usages;

		private LanguagePlugin _plugin;

		public CommandHandlerParameter Usage { 
			get {
				if (_usages == null) {
					var usages = new CommandHandlerParameter(
						Command,
						CommandType.LanguageCommand,
						Command,
						"Commands for the " + Command + " plugin");
					_plugin.GetUsages().ToList()
						.ForEach(x => usages.Add(x));
					_usages = usages;
				}
				return _usages;
			}
		}

		public string Command { get; private set; }

		public LanguageHandler(LanguagePlugin plugin)
		{
			_plugin = plugin;
			Command = _plugin.GetLanguage();
		}

		public void Execute(string[] arguments)
		{
			_plugin.Run(arguments);
		}
	}
}
