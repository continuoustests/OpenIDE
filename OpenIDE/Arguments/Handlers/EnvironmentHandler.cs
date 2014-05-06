using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.CodeEngineIntegration;
using OpenIDE.Core.EditorEngineIntegration;
using OpenIDE.Core.Environments;

namespace OpenIDE.Arguments.Handlers
{
	class EnvironmentHandler : ICommandHandler
	{
		private Action<string> _dispatch;
		private EnvironmentService _environment;
		private ICodeEngineLocator _locator;
		private ILocateEditorEngine _editorLocator;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Lists and manages OpenIDE evironments");
				usage
					.Add("details", "Displays details about a running environment")
						.Add("KEY", "The environment key path");
				usage.Add("start", "Starts OpenIDE for the current path (no editor engine)");
				return usage;
			}
		}

		public string Command { get { return "environment"; } }

		public EnvironmentHandler(Action<string> dispatch, ICodeEngineLocator locator, ILocateEditorEngine editorLocator, EnvironmentService environment) {
			_dispatch = dispatch;
			_locator = locator;
			_editorLocator = editorLocator;
			_environment = environment;
		}

		public void Execute(string[] arguments) {
			if (arguments.Length == 0)
				list();
			if (arguments.Length == 2 && arguments[0] == "details")
				details(arguments[1]);
			if (arguments.Length == 1 && arguments[0] == "start")
				_environment.Start(Environment.CurrentDirectory);
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
			_dispatch("Key:    " + instance.Key);
			_dispatch(string.Format("Engine: Pid {0} @ 127.0.0.1:{1}", instance.ProcessID, instance.Port));
			var events = new OpenIDE.Core.EventEndpointIntegrarion.EventClient(key).GetInstance();
			if (events != null) {
				_dispatch(string.Format("Events: Pid {0} @ 127.0.0.1:{1}", events.ProcessID, events.Port));
			}
			var output = new OpenIDE.Core.OutputEndpointIntegration.OutputClient(key).GetInstance();
			if (output != null) {
				_dispatch(string.Format("Output: Pid {0} @ 127.0.0.1:{1}", output.ProcessID, output.Port));
			}
			var editor
				= _editorLocator.GetInstances()
					.FirstOrDefault(x => x.Key == instance.Key);
			if (editor == null) {
				_dispatch("Editor: Not running");
				return;
			}
			_dispatch(string.Format("Editor: Pid {0} @ 127.0.0.1:{1}", editor.ProcessID, editor.Port));
		}

		private bool matchPath(string key, string path) {
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
				return key == path;
			else
				return key.ToLower() == path.ToLower();
		}
	}
}