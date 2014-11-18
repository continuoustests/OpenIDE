using System;
using System.Linq;
using OpenIDE.Core.Language;
using OpenIDE.Core.RScripts;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.CodeEngineIntegration;

namespace OpenIDE.Arguments.Handlers
{
	public class ListReactiveScriptsHandler : ICommandHandler
	{
		private string _token;
		private Func<PluginLocator> _pluginLocator;
		private ICodeEngineLocator _codeEngineLocator;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Lists reactive scripts");
					return usage;
			}
		}
	
		public string Command { get { return "list"; } }

		public ListReactiveScriptsHandler(string token, Func<PluginLocator> pluginLocator, ICodeEngineLocator codeEngineLocator)
		{
			_token = token;
			_pluginLocator = pluginLocator;
			_codeEngineLocator = codeEngineLocator;
		}

		public void Execute(string[] arguments)
		{
			var showState = arguments.Any(x => x == "-s");
			var scripts = 
				new ReactiveScriptReader(
					_token,
					_pluginLocator,
					(p, m) => {},
					(m) => {})
					.Read();
			if (!showState) {
				foreach (var script in scripts)
					Console.WriteLine(script.Name);
				return;
			}
			using (var instance = _codeEngineLocator.GetInstance(_token)) {
				foreach (var script in scripts) {
					var status = "unavailable";
					if (instance != null)
						status = instance.GetRScriptState(script.Name);
					Console.WriteLine(script.Name + " (" + status + ")");
				}
			}
		}
	}
}