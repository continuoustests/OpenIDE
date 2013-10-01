using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenIDE.Core.Language;
using OpenIDE.Core.RScripts;

namespace OpenIDE.Arguments.Handlers
{
	class DeleteReactiveScriptHandler : ICommandHandler
	{
		private string _keyPath;
		private Func<PluginLocator> _pluginLocator;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Deletes a reactive script");
					usage.Add("SCRIPT-NAME", "Reactive script name. Local are picked over global");
					return usage;
			}
		}
	
		public string Command { get { return "rm"; } }

		public DeleteReactiveScriptHandler(Func<PluginLocator> locator, string keyPath)
		{
			_keyPath = keyPath;
			_pluginLocator = locator;
		}
	
		public void Execute(string[] arguments)
		{
			var scripts = new ReactiveScriptReader(
				_keyPath,
				_pluginLocator,
				(m) => {})
				.ReadNonLanguageScripts();
			var script = scripts.FirstOrDefault(x => x.Name.Equals(arguments[0]));
			if (script == null || arguments.Length < 1)
				return;
			File.Delete(script.File);
		}
	}
}
