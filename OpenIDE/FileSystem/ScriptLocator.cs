using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Scripts;

namespace OpenIDE.FileSystem
{
	class ScriptLocator
	{
		public string GetTemplateFor(string extension)
		{
			if (extension == null)
				return null;
			var dir =
				Path.Combine(
					GetGlobalPath(),
					"templates");
			if (!Directory.Exists(dir))
				return null;
			return new ScriptFilter().GetScripts(dir)
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
			return getPath(Path
					.GetDirectoryName(
						Assembly.GetExecutingAssembly().Location));
		}

		public string GetLocalPath()
		{
			var configFile = Configuration.GetConfigFile(Environment.CurrentDirectory);
			if (configFile == null)
				return null;
			return getPath(Path.GetDirectoryName(configFile));
		}

		private string getPath(string location)
		{
			return Path.Combine(location, "scripts");
		}

		private IEnumerable<Script> getScripts(string path)
		{
			if (!Directory.Exists(path))
				return new Script[] {};
			return Directory.GetFiles(path)
				.Select(x => new Script(x));
		}
	}
}
