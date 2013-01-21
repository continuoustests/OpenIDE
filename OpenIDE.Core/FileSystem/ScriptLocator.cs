using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Scripts;
using OpenIDE.Core.Profiles;

namespace OpenIDE.Core.FileSystem
{
	public class ReactiveScriptLocator : TemplateLocator 
	{
		public ReactiveScriptLocator()
		{
			_directory = "rscripts";
		}
	}
	
	public class ScriptLocator : TemplateLocator
	{
		public ScriptLocator()
		{
			_directory = "scripts";
		}
	}

	public class TemplateLocator 
	{
		protected string _directory;

		public IEnumerable<string> GetTemplates()
		{
			var dir =
				Path.Combine(
					GetGlobalPath(),
					"templates");
			if (!Directory.Exists(dir))
				return null;
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

		public Script[] GetGlobalScripts()
		{
			return getScripts(GetGlobalPath()).ToArray();
		}

		public Script[] GetLocalScripts()
		{
			return getScripts(GetLocalPath()).ToArray();
		}

		public string GetGlobalPath()
		{
			var locator = new ProfileLocator(_directory);
			return getPath(locator.GetGlobalProfilePath(locator.GetActiveGlobalProfile()));
		}

		public string GetLocalPath()
		{
			var configFile = Configuration.GetConfigFile(Environment.CurrentDirectory);
			if (configFile == null)
				return null;
			return getPath(Path.GetDirectoryName(configFile));
		}

		public string GetLanguagePath(string language)
		{
			return getPath(
				Path.Combine(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
					Path.Combine("Languages", language + "-plugin")));
		}

		private string getPath(string location)
		{
			return Path.Combine(location, _directory);
		}

		private IEnumerable<Script> getScripts(string path)
		{
			if (!Directory.Exists(path))
				return new Script[] {};
			var workingDir = Path.GetDirectoryName(Path.GetDirectoryName(path));
			return new ScriptFilter().GetScripts(path)
				.Select(x => new Script(workingDir, x));
		}
	}
}
