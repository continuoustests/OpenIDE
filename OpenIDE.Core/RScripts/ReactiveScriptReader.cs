using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Scripts;
using OpenIDE.Core.Language;

namespace OpenIDE.Core.RScripts
{
	public class ReactiveScriptReader
	{
		private string _keyPath;
		private string _oiRootPath;
		private List<ReactiveScript> _scripts = new List<ReactiveScript>();
		private Func<PluginLocator> _pluginLocator;

		public ReactiveScriptReader(string oiRootPath, Func<PluginLocator> locator)
		{
			_oiRootPath = oiRootPath;
			_pluginLocator = locator;
		}

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
			_pluginLocator().Locate().ToList()
				.ForEach(plugin => 
					readScripts(
						Path.Combine(
							_oiRootPath,
							Path.Combine(
								"Languages",
								Path.Combine(
									plugin.GetLanguage(),
									"rscripts")))));
			readScripts(getGlobal());
		}
		
		private string getGlobal()
		{
				return getPath(_oiRootPath);
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
			return Path.Combine(path, "rscripts");
		}
	}
}
