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
		private List<LanguagePlugin> _plugins = new List<LanguagePlugin>();

		public PluginLocator(string[] enabledLanguages, ProfileLocator profiles, Action<string> dispatchMessage)
		{
			_enabledLanguages = enabledLanguages;
			_profiles = profiles;
			_dispatchMessage = dispatchMessage;
		}

		public LanguagePlugin[] Locate()
		{
			return getPlugins()
				.Select(x => getPlugin(x))
				.Where(x => _enabledLanguages == null || _enabledLanguages.Contains(x.GetLanguage()))
				.ToArray();
		}

		private LanguagePlugin getPlugin(string name) {
			var plugin = _plugins.FirstOrDefault(x => x.FullPath == name);
			if (plugin == null) {
				plugin = new LanguagePlugin(name, _dispatchMessage);
				_plugins.Add(plugin);
			}
			return plugin;
		}
		
		public IEnumerable<BaseCommandHandlerParameter> GetUsages()
		{
			var commands = new List<BaseCommandHandlerParameter>();
			Locate().ToList()
				.ForEach(plugin => 
					{
						plugin.GetUsages().ToList()
							.ForEach(y => commands.Add(y));
					});
			return commands;
		}

		private string[] getPlugins()
		{
			var plugins = new List<string>();
			var dirs = 
				new[] {
					Path.Combine(
						_profiles.GetLocalProfilePath(
							_profiles.GetActiveLocalProfile()), "Languages"),
					Path.Combine(
						_profiles.GetLocalProfilePath(
							"default"), "Languages"),
					Path.Combine(
						_profiles.GetGlobalProfilePath(
							_profiles.GetActiveGlobalProfile()), "Languages"),
					Path.Combine(
						_profiles.GetGlobalProfilePath(
							"default"), "Languages")
				};
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

		private IEnumerable<string> getPlugins(string dir)
		{
			if (!Directory.Exists(dir))
				return new string[] {};
			return Directory.GetFiles(dir);	
		}
	}
}
