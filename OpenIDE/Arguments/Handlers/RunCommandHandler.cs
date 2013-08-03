using System;
using System.Linq;
using OpenIDE.UI;
using OpenIDE.CommandBuilding;
using System.Collections.Generic;
using System.IO;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Language;
using System.Windows.Forms;

namespace OpenIDE.Arguments.Handlers
{
	class RunCommandHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Launches the command execution window");
				return usage;
			}
		}

		public string Command { get { return "run"; } }

		public void Execute (string[] arguments)
		{
			var form = new RunCommandForm(
				Directory.GetCurrentDirectory(),
				Bootstrapper.Settings.DefaultLanguage,
				new CommandBuilder(Bootstrapper.GetDefinitionBuilder().Definitions));
			form.ShowDialog();
		}
	}
}
