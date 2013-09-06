using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.EditorEngineIntegration;

namespace OpenIDE.Arguments.Handlers
{
	class EnvironmentHandler : ICommandHandler
	{
		private Action<string> _dispatch;
		private ICodeEngineLocator _locator;
		private ILocateEditorEngine _editorLocator;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Lists and manages running OpenIDE evironments");
				usage
					.Add("details", "Displays details about a running environment")
						.Add("KEY", "The environment key path");
				return usage;
			}
		}

		public string Command { get { return "environment"; } }

		public EnvironmentHandler(Action<string> dispatch, ICodeEngineLocator locator, ILocateEditorEngine editorLocator) {
			_dispatch = dispatch;
			_locator = locator;
			_editorLocator = editorLocator;
		}

		public void Execute(string[] arguments) {
			if (arguments.Length == 0)
				list();
			if (arguments.Length == 2 && arguments[0] == "details")
				details(arguments[1]);
		}

		private void list() {
			var instances = _locator.GetInstances();
			foreach (var instance in instances)
				_dispatch(instance.Key);
		}

		private void details(string key) {
			var instance = _locator.GetInstances().FirstOrDefault(x => matchPath(key, x.Key));
			if (instance == null)
				return;
			_dispatch("Key:\t\t" + instance.Key);
			_dispatch(string.Format("Endpoint:\tPid {0} @ 127.0.0.1:{1}", instance.ProcessID, instance.Port));
			var editor
				= _editorLocator.GetInstances()
					.FirstOrDefault(x => x.Key == instance.Key);
			if (editor == null) {
				_dispatch("Editor:\t\tNot running");
				return;
			}
			_dispatch(string.Format("Editor:\t\tPid {0} @ 127.0.0.1:{1}", editor.ProcessID, editor.Port));
		}

		private bool matchPath(string key, string path) {
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
				return key == path;
			else
				return key.ToLower() == path.ToLower();
		}
	}
}