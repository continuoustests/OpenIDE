using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;

namespace OpenIDE.Core.Configs
{
	public class Interpreters
	{
		private string _token;
		private Dictionary<string,string> _interpreters = new Dictionary<string,string>();

		public Interpreters(string token) {
			_token = token;
			var local = new Configuration(_token, false);
			var global = 
				new Configuration(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
					, false);
			readInterpreters(local);
			readInterpreters(global);
		}

		public string GetInterpreterFor(string extension) {
			string interpreter;
			if (_interpreters.TryGetValue(extension, out interpreter))
				return interpreter;
			return null;
		}

		private void readInterpreters(Configuration config) {
			var prefix = "interpreter.";
			foreach (var interpreter in config.GetSettingsStartingWith(prefix)) {
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
					path = 
						System.IO.Path.Combine(
							System.IO.Path.GetDirectoryName(
								Assembly.GetExecutingAssembly().Location),
							path);
					if (!File.Exists(path))
						continue;
				}
				if (!_interpreters.ContainsKey(extension))
					_interpreters.Add(extension, path);
			}
		}
	}
}