using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace OpenIDE.Core.Scripts
{
	public class ScriptFilter
	{
		public IEnumerable<string> GetScripts(string path)
		{
			if (!Directory.Exists(path))
				return new string[] {};
			return 
				FilterScripts(
					Directory
						.GetFiles(path));
		}

		public IEnumerable<string> FilterScripts(IEnumerable<string> scripts)
		{
			return 	scripts
				.Where(x => {
					if (Environment.OSVersion.Platform == PlatformID.Unix ||
						Environment.OSVersion.Platform == PlatformID.MacOSX)
					{
						if (Path.GetExtension(x).ToLower() == ".bat")
							return false;
					}
					else
					{
						if (Path.GetExtension(x).ToLower() == ".sh")
							return false;
					}
					return true;
				});
		}
	}
}
