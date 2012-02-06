using System;
using System.Linq;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class LanguageHandler : ICommandHandler
	{
		private LanguagePlugin _plugin;

		public CommandHandlerParameter Usage { get; private set; }

		public string Command { get; private set; }

		public LanguageHandler(LanguagePlugin plugin)
		{
			_plugin = plugin;
			Command = _plugin.GetLanguage();
			Usage = new CommandHandlerParameter(
				Command,
				CommandType.LanguageCommand,
				Command,
				"Commands for the " + Command + " plugin");
			_plugin.GetUsages().ToList()
				.ForEach(x => Usage.Add(x));
		}

		public void Execute(string[] arguments)
		{
			_plugin.Run(arguments);
		}
	}
}
