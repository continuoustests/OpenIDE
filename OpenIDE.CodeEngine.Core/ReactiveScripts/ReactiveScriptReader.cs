using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;

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
			Logging.Logger.Write("Reading scripts from " + path);
			if (!Directory.Exists(path))
				return;
			_scripts.AddRange(
				Directory.GetFiles(path, "*.rscript")
					.Select(x => ReadScript(path))
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
				return getPath(Path.GetDirectoryName(
					Configuration.GetConfigFile(_keyPath)));
		}

		private string getPath(string path)
		{
			return Path.Combine(path, "reactive-scripts");
		}
	}
}
