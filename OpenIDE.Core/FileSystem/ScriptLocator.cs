using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Scripts;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.Language;

namespace OpenIDE.Core.FileSystem
{
	public class ReactiveScriptLocator : TemplateLocator 
	{
		public ReactiveScriptLocator(string keyPath, string currentDirectory) : base(keyPath, currentDirectory) {
			_directory = "rscripts";
		}
	}
	
	public class ScriptLocator : TemplateLocator
	{
		public ScriptLocator(string keyPath, string currentDirectory) : base(keyPath, currentDirectory) {
			_directory = "scripts";
		}
	}
	
	public class TestTemplateLocator : TemplateLocator
	{
		public TestTemplateLocator(string keyPath, string currentDirectory) : base(keyPath, currentDirectory) {
			_directory = "test";
		}
	}

	public class TemplateLocator 
	{
		protected string _directory;
		protected string _keyPath;
		protected string _currentDirectory;

		public TemplateLocator(string keyPath, string currentDirectory) {
			_keyPath = keyPath;
			_currentDirectory = currentDirectory;
		}

		public IEnumerable<string> GetTemplates()
		{
			var dir =
				Path.Combine(
					GetGlobalPath("default"),
					"templates");
			if (!Directory.Exists(dir))
				return new string[] {};
			return new ScriptFilter().GetScripts(dir);
		}

		public string GetTemplateFor(string extension)
		{
			if (extension == null)
				return null;
			return GetTemplates()
				.Where(x => x.EndsWith(extension))
				.FirstOrDefault();
		}

		public Script[] GetGlobalScripts(string profile)
		{
			return getScripts(GetGlobalPath(profile)).ToArray();
		}

		public Script[] GetGlobalScripts()
		{
			var defaultPath = getPath(GetGlobalPath("default"));
			var profilePath = GetGlobalPath();
			return getMergedScripts(defaultPath, profilePath);
		}

		public Script[] GetLocalScripts(string profile)
		{
			return getScripts(GetLocalPath(profile)).ToArray();
		}

		public Script[] GetLocalScripts()
		{
			var defaultPath = GetLocalPath("default");
			if (defaultPath != null)
				defaultPath = getPath(defaultPath);
			var profilePath = GetLocalPath();
			return getMergedScripts(defaultPath, profilePath);
		}

		public string GetGlobalPath()
		{
			var locator = new ProfileLocator(_keyPath);
			return GetGlobalPath(locator.GetActiveGlobalProfile());
		}

		public string GetGlobalPath(string profile)
		{
			var locator = new ProfileLocator(_keyPath);
			return getPath(locator.GetGlobalProfilePath(profile));
		}

		public string GetLocalPath()
		{
			var locator = new ProfileLocator(_keyPath);
			return GetLocalPath(locator.GetActiveLocalProfile());
		}

		public string GetLocalPath(string profile)
		{
			var locator = new ProfileLocator(_keyPath);
			var profilePath = locator.GetLocalProfilePath(profile);
			if (profilePath == null)
				return profilePath;
			return getPath(profilePath);
		}

		private string getPath(string location)
		{
			location = Path.GetFullPath(location);
			return Path.Combine(location, _directory);
		}

		private Script[] getMergedScripts(string defaultPath, string profilePath) {
			var scripts = new List<Script>();
			scripts.AddRange(getScripts(defaultPath));
			if (profilePath != defaultPath)
				scripts.AddRange(getScripts(profilePath));
			return scripts.ToArray();
		}

		private IEnumerable<Script> getScripts(string path)
		{
			if (path == null)
				return new Script[] {};
			if (!Directory.Exists(path))
				return new Script[] {};
			path = Path.GetFullPath(path);
			return new ScriptFilter().GetScripts(path)
				.Select(x => new Script(_keyPath, _currentDirectory, x));
		}
	}
}