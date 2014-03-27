using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.Configs
{
	public class Interpreters
	{
		private string _token;
		private Dictionary<string,string> _interpreters = new Dictionary<string,string>();

		public Interpreters(string token) {
            Logger.Write("Initializing interpreters");
			_token = token;
			var reader = new ConfigReader(_token);
			readInterpreters(reader);
		}

		public string GetInterpreterFor(string extension) {
			string interpreter;
			if (_interpreters.TryGetValue(extension, out interpreter))
				return interpreter;
			return null;
		}

		private void readInterpreters(ConfigReader config) {
			var prefix = "interpreter.";
			foreach (var interpreter in config.GetStartingWith(prefix)) {
				if (interpreter.Key.Length <= prefix.Length)
					continue;
				
				var extension = 
					interpreter.Key
						.Substring(
							prefix.Length,
							 interpreter.Key.Length - prefix.Length);
				if (extension.Length == 0)
					continue;

				extension = "." + extension;
				var path = interpreter.Value;
				if (Environment.OSVersion.Platform != PlatformID.Unix &&
					Environment.OSVersion.Platform != PlatformID.MacOSX) {
					path.Replace("/", "\\");
				}
				if (!File.Exists(path)) {
					var modifiedPath = 
						System.IO.Path.Combine(
							System.IO.Path.GetDirectoryName(
								Assembly.GetExecutingAssembly().Location),
							path);
					if (File.Exists(path))
						path = modifiedPath;
				}
				if (!_interpreters.ContainsKey(extension)) {
                    Logger.Write("Adding interpreter: " + path + " for " + extension);
					_interpreters.Add(extension, path);
                }
			}
		}
	}
}
