using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using CoreExtensions;

namespace OpenIDE.Core.Language
{
	public class PluginLocator
	{
		private string[] _enabledLanguages;
		private string _languageRoot;
		private Action<string> _dispatchMessage;

		public PluginLocator(string[] enabledLanguages, string languageRoot, Action<string> dispatchMessage)
		{
			_enabledLanguages = enabledLanguages;
			_languageRoot = languageRoot;
			_dispatchMessage = dispatchMessage;
		}

		public LanguagePlugin[] Locate()
		{
			return getPlugins()
				.Select(x => new LanguagePlugin(x, run, _dispatchMessage))
				.Where(x => _enabledLanguages == null || _enabledLanguages.Contains(x.GetLanguage()))
				.ToArray();
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
			var dir = 
				Path.Combine(
					_languageRoot,
					"Languages");
			if (!Directory.Exists(dir))
				return new string[] {};
			return Directory.GetFiles(dir);
		}

		private IEnumerable<string> run(string cmd, string arguments)
		{
			var proc = new Process();
            return proc.Query(cmd, arguments, false, Environment.CurrentDirectory);
		}
	}
}
