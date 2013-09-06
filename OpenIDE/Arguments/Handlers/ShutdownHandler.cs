using System;
using System.IO;
using System.Diagnostics;
using OpenIDE.Core.Language;
using OpenIDE.Core.EditorEngineIntegration;

namespace OpenIDE.Arguments.Handlers
{
	class ShutdownHandler : ICommandHandler
	{
		private string _rootPath;
		private Action<string> _dispatch;
		private ILocateEditorEngine _editorLocator;

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

		public ShutdownHandler(string rootPath, Action<string> dispatch, ILocateEditorEngine editorLocator) {
			_rootPath = rootPath;
			_dispatch = dispatch;
			_editorLocator = editorLocator;
		}

		public void Execute(string[] arguments) {
			var path = _rootPath;
			if (arguments.Length == 1 && Directory.Exists(arguments[0]))
				path = arguments[0];
			var instance = _editorLocator.GetInstance(path);
			if (instance == null)
				return;
			var process = Process.GetProcessById(instance.ProcessID);
			process.Kill();
		}
	}
}