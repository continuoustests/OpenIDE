using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenIDE.Core.Language;
using OpenIDE.Core.RScripts;

namespace OpenIDE.Arguments.Handlers
{
	class EditReactiveScriptHandler : ICommandHandler
	{
		private string _keyPath;
		private Action<string> _dispatch;
		private PluginLocator _pluginLocator;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Opens an existing reactive script for editor");
					usage.Add("SCRIPT-NAME", "Reactive script name. Local are picked over global");
					return usage;
			}
		}
	
		public string Command { get { return "edit"; } }
		
		public EditReactiveScriptHandler(Action<string> dispatch, PluginLocator locator, string keyPath)
		{
			_dispatch = dispatch;
			_pluginLocator = locator;
			_keyPath = keyPath;
		}
	
		public void Execute(string[] arguments)
		{
			var scripts = new ReactiveScriptReader(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				_keyPath,
				() => { return _pluginLocator; },
				(m) => {})
				.Read();
			var script = scripts.FirstOrDefault(x => x.Name.Equals(arguments[0]));
			if (script == null || arguments.Length < 1)
				return;
			_dispatch(string.Format("editor goto \"{0}|0|0\"", script.File));
		}
	}
}
