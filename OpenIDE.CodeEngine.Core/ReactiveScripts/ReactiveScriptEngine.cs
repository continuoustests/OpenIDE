using System;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.RScripts;

namespace OpenIDE.CodeEngine.Core.ReactiveScripts
{
	public class ReactiveScriptEngine
	{
		private string _keyPath;
		private List<ReactiveScript> _scripts;	

		public ReactiveScriptEngine(string path)
		{
			_keyPath = path;
			_scripts = new ReactiveScriptReader().Read(_keyPath);
		}

		public void Handle(string message)
		{
			_scripts
				.Where(x => x.ReactsTo(message)).ToList()
				.ForEach(x => x.Run(message));
		}
	}
}
