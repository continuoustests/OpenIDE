using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Logging;
using OpenIDE.Core.RScripts;
using OpenIDE.Core.Language;

namespace OpenIDE.CodeEngine.Core.ReactiveScripts
{
	public class ReactiveScriptEngine
	{
		private string _keyPath;
		private List<ReactiveScript> _scripts;	

		public ReactiveScriptEngine(string path, PluginLocator locator)
		{
			_keyPath = path;
			_scripts = new ReactiveScriptReader(
				Path.GetDirectoryName(
					Path.GetDirectoryName(
						Assembly.GetExecutingAssembly().Location)),
				() => { return locator; })
				.Read(_keyPath);
		}

		public void Handle(string message)
		{
			_scripts
				.Where(x => x.ReactsTo(message)).ToList()
				.ForEach(x => x.Run(message));
		}
	}
}
