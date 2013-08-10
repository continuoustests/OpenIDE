using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Profiles;

namespace OpenIDE.Core.Language
{
	public class PluginLocator
	{
		private string[] _enabledLanguages;
		private ProfileLocator _profiles;
		private Action<string> _dispatchMessage;
		private List<LanguagePlugin> _plugins = null; 
		private object _padlock = new object();

		public PluginLocator(string[] enabledLanguages, ProfileLocator profiles, Action<string> dispatchMessage)
		{
			_enabledLanguages = enabledLanguages;
			_profiles = profiles;
			_dispatchMessage = dispatchMessage;
		}

		public LanguagePlugin[] Locate()
		{
			if (_plugins == null) {
				lock (_padlock) {
					_plugins = new List<LanguagePlugin>();
					foreach (var file in getPlugins()) {
						var plugin = new LanguagePlugin(file, _dispatchMessage);
						if (isEnabledPlugin(plugin))
							_plugins.Add(plugin);
					}
				}
			}
			return _plugins.ToArray();
		}

		public LanguagePlugin[] LocateFor(string path)
		{
			return
				Locate()
					.Where(x => x.FullPath.StartsWith(path)) 
					.ToArray();
		}

		public IEnumerable<BaseCommandHandlerParameter> GetUsages()
		{
			var commands = new List<BaseCommandHandlerParameter>();
			foreach (var plugin in Locate())
				commands.AddRange(plugin.GetUsages());
			return commands;
		}

		private bool isEnabledPlugin(LanguagePlugin plugin)
		{
			return _enabledLanguages == null || _enabledLanguages.Contains(plugin.GetLanguage());
		}

		private string[] getPlugins()
		{
			var plugins = new List<string>();
			var dirs = new List<string>();
			
			addLanguagePath(
				dirs,
				_profiles.GetLocalProfilePath(_profiles.GetActiveLocalProfile()));
			addLanguagePath(
				dirs,
				_profiles.GetLocalProfilePath("default"));
			addLanguagePath(
				dirs,
				_profiles.GetGlobalProfilePath(_profiles.GetActiveGlobalProfile()));
			addLanguagePath(
				dirs,
				_profiles.GetGlobalProfilePath("default"));
			
			foreach (var dir in dirs) {
				var list = getPlugins(dir);
				plugins.AddRange(
						list
							.Where(x => 
								!plugins.Any(y => Path.GetFileNameWithoutExtension(y) == Path.GetFileNameWithoutExtension(x)))
							.ToArray());
			}
			return plugins.ToArray();
		}

		private void addLanguagePath(List<string> dirs, string path) {
			if (path == null)
				return;
			dirs.Add(Path.Combine(path, "languages"));
		}

		private IEnumerable<string> getPlugins(string dir)
		{
			if (!Directory.Exists(dir))
				return new string[] {};
			return Directory.GetFiles(dir);	
		}
	}
}
