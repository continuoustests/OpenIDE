using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Scripts;

namespace OpenIDE.CodeEngine.Core.ReactiveScripts
{
	class ReactiveScriptReader
	{
		private string _keyPath;
		private List<ReactiveScript> _scripts = new List<ReactiveScript>();

		public List<ReactiveScript> Read(string path)
		{
			_keyPath = path;
			readScripts();
			return _scripts;
		}

		public ReactiveScript ReadScript(string path)
		{
			return new ReactiveScript(path, _keyPath);
		}
		
		private void readScripts(string path)
		{
			if (path == null)
				return;
			Logging.Logger.Write("Reading scripts from " + path);
			if (!Directory.Exists(path))
				return;
			_scripts.AddRange(
				new ScriptFilter().GetScripts(path)
					.Select(x => ReadScript(x))
					.Where(x => x != null));
		}

		private void readScripts()
		{
			readScripts(getLocal());
			readScripts(getGlobal());
		}
		
		private string getGlobal()
		{
				return getPath(
					Path.GetDirectoryName(
						Path.GetDirectoryName(
							Assembly.GetExecutingAssembly().Location)));
		}

		private string getLocal()
		{
			var config = Configuration.GetConfigFile(_keyPath);
			if (config == null)
				return null;
			return getPath(Path.GetDirectoryName(config));
		}

		private string getPath(string path)
		{
			return Path.Combine(path, "reactive-scripts");
		}
	}
}
