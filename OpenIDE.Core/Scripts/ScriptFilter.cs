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
			return 	scripts.Where(x => IsValid(x));
		}

        public bool IsValid(string filepath)
        {
            var x = filepath;
            if (Path.GetFileName(x).StartsWith("."))
                return false;
            if (Path.GetExtension(x).ToLower() == ".swp")
                return false;
            if (Path.GetExtension(x).EndsWith("~"))
                return false;
            if (Path.GetExtension(x).ToLower() == ".swo")
                return false;
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
        }
	}
}
