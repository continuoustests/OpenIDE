using System;
using System.Linq;
using OpenIDE.Core.Language;
using OpenIDE.Core.RScripts;
using OpenIDE.Core.FileSystem;

namespace OpenIDE.Arguments.Handlers
{
	public class ListReactiveScriptsHandler : ICommandHandler
	{
		private string _token;
		private PluginLocator _pluginLocator;

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

		public ListReactiveScriptsHandler(string token, PluginLocator pluginLocator)
		{
			_token = token;
			_pluginLocator = pluginLocator;
		}

		public void Execute(string[] arguments)
		{
			var scripts = 
				new ReactiveScriptReader(
					_token,
					() => { return _pluginLocator; },
					(m) => {})
					.Read();
			foreach (var script in scripts)
				Console.WriteLine(script.Name);
		}
	}
}