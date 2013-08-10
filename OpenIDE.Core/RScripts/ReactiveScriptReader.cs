using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Scripts;
using OpenIDE.Core.Language;
using OpenIDE.Core.Profiles;

namespace OpenIDE.Core.RScripts
{
	public class ReactiveScriptReader
	{
		private string _keyPath;
		private string _localScriptsPathDefault;
		private string _localScriptsPath;
		private string _globalScriptsPathDefault;
		private string _globalScriptsPath;
		private List<ReactiveScript> _scripts = new List<ReactiveScript>();
		private Func<PluginLocator> _pluginLocator;
		private Action<string> _dispatch;

		public ReactiveScriptReader(string path, Func<PluginLocator> locator, Action<string> dispatch)
		{
			_keyPath = path;
			_dispatch = dispatch;
			var profiles = new ProfileLocator(_keyPath);
			_localScriptsPathDefault = getPath(profiles.GetLocalProfilePath("default"));
			_localScriptsPath = getPath(profiles.GetLocalProfilePath(profiles.GetActiveLocalProfile()));
			_globalScriptsPathDefault = getPath(profiles.GetGlobalProfilePath("default"));
			_globalScriptsPath = getPath(profiles.GetGlobalProfilePath(profiles.GetActiveGlobalProfile()));
			_pluginLocator = locator;
		}

		public List<ReactiveScript> Read()
		{
			readScripts();
			return _scripts;
		}

		public List<ReactiveScript> ReadNonLanguageScripts()
		{
			if (_scripts.Count == 0)
				Read();
			var paths = GetPathsNoLanguage();
			return
				_scripts
					.Where(x => paths.Any(y => x.File.StartsWith(y)))
					.ToList();
		}

		public List<ReactiveScript> ReadLanguageScripts()
		{
			if (_scripts.Count == 0)
				Read();
			var paths = getLanguagePaths();
			return
				_scripts
					.Where(x => paths.Any(y => x.File.StartsWith(y)))
					.ToList();
		}

		public ReactiveScript ReadScript(string path)
		{
            try {
			    return new ReactiveScript(path, _keyPath, _dispatch);
            } catch (Exception ex) {
                Logger.Write(ex);
            }
            return null;
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
			foreach (var path in GetPaths()) {
				Logger.Write("Reading scripts from: " + path);
				readScripts(path);
			}
		}

		public List<string> GetPaths()
		{
			var paths = new List<string>();
			addToList(paths, getLocal());
			addToList(paths, _localScriptsPathDefault);
			foreach (var path in getLanguagePaths())
				addToList(paths, path);
			addToList(paths, getGlobal());
			addToList(paths, _globalScriptsPathDefault);
			return paths;
		}

		public List<string> GetPathsNoLanguage()
		{
			var paths = new List<string>();
			addToList(paths, getLocal());
			addToList(paths, _localScriptsPathDefault);
			addToList(paths, getGlobal());
			addToList(paths, _globalScriptsPathDefault);
			return paths;
		}

		private List<string> getLanguagePaths() {
			var paths = new List<string>();
			foreach (var plugin in _pluginLocator().Locate()) {
				paths.Add(
					Path.Combine(
						Path.GetDirectoryName(plugin.FullPath),
						Path.Combine(
							plugin.GetLanguage() + "-files",
							"rscripts")));
			}
			return paths;
		}

		private void addToList(List<string> list, string item)
		{
			if (item == null || item.Length == 0)
				return;
			if (!list.Contains(item))
				list.Add(item);
		}
		
		private string getGlobal()
		{
			return _globalScriptsPath;
		}

		private string getLocal()
		{
			return _localScriptsPath;
		}

		private string getPath(string path)
		{
			if (path == null)
				return null;
			return Path.Combine(path, "rscripts");
		}
	}
}
