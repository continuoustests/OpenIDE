using System;
using OpenIDENet.Languages;
using OpenIDENet.UI;

namespace OpenIDENet.Arguments.Handlers
{
	class RunCommandHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					SupportedLanguage.All,
					CommandType.Run,
					Command,
					"Launches the command execution window");
				return usage;
			}
		}

		public string Command { get { return "run"; } }
		
		public void Execute (string[] arguments, Func<string, ProviderSettings> getTypesProviderByLocation)
		{
			var form = new RunCommandForm();
			form.ShowDialog();
		}
	}
}
