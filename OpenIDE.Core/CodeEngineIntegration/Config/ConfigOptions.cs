using System;
using System.IO;
using System.Collections.Generic;

namespace OpenIDE.Core.Config
{
	public class ConfigOptionsReader
	{
		private string[] _paths;
		private List<string> _options = new List<string>();

		public string[] Options { get { return _options.ToArray(); }}

		public ConfigOptionsReader(string[] paths) {
			_paths = paths;
		}

		public void Parse() {
			addOption("default.language", "Default configured language");
			addOption("enabled.languages", "Languages enabled within this configuration");
			foreach (var path in _paths) {
				var files = Directory.GetFiles(path, "*.oicfgoptions");
				foreach (var file in files) {
					var lines = File.ReadAllLines(file);
					foreach (var line in lines) {
						var chunks = line.Split(new[] { '|' });
						if (chunks.Length == 2)
							addOption(chunks[0], chunks[1]);
					}
				}
			}
		}

		private void addOption(string option, string comment) {
			_options.Add(option.PadRight(40, ' ') + "// " + comment);
		}
	}
}