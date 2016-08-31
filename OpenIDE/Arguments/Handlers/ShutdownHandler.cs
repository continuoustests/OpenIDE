using System;
using System.IO;
using System.Diagnostics;
using OpenIDE.Bootstrapping;
using OpenIDE.Core.Environments;
using OpenIDE.Core.Language;
using OpenIDE.Core.EditorEngineIntegration;
using OpenIDE.Core.CodeEngineIntegration;

namespace OpenIDE.Arguments.Handlers
{
	class ShutdownHandler : ICommandHandler
	{
		private string _rootPath;
		private EnvironmentService _environment;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Shuts down current environment but leaves editor");
				usage.Add("[KEY]", "Key path of environment to shut down");
				return usage;
			}
		}

		public string Command { get { return "shutdown"; } }

		public ShutdownHandler(string rootPath, Action<string> dispatch, EnvironmentService environment) {
			_rootPath = rootPath;
			_environment = environment;
		}

		public void Execute(string[] arguments) {
			var path = _rootPath;
			if (arguments.Length == 1 && Directory.Exists(arguments[0]))
				path = arguments[0];
			_environment.Shutdown(path);
		}
	}
}