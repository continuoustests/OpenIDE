using System;
using System.IO;
using System.Linq;

namespace OpenIDENet.Core.Config
{
	public class Configuration
	{
		private const string DEFAULT_LANGUAGE_SETTING = "default-language=";

		private string _path;

		public string DefaultLanguage { get; private set; }

		public Configuration(string path)
		{
			_path = path;
			readConfiguration();
		}

		public void Write(string setting)
		{
			if (!isSetting(setting))
			{
				Console.WriteLine("Invalid setting: " + setting);
				return;
			}
			var file = getConfigFile();
			if (!Directory.Exists(Path.GetDirectoryName(file)))
				Directory.CreateDirectory(Path.GetDirectoryName(file));
			string[] lines = new string[] {};
			if (File.Exists(file))
				lines = File.ReadAllLines(file);
			write(file, lines, setting);
		}


		private void write(string file, string[] lines, string setting)
		{
			var settingTag = getTag(setting);
			using (var writer = new StreamWriter(file))
			{
				var written = false;
				lines.ToList()
					.ForEach(x => 
						{
							if (getTag(x).Equals(settingTag))
							{
								writer.WriteLine(setting);
								written = true;
							}
							else
								writer.WriteLine(x);
						});
				if (!written)
					writer.WriteLine(setting);
			}
		}

		private string getTag(string setting)
		{
			var check = setting 
				.Replace(" ", "")
				.Replace("\t", "");
			var space = setting.IndexOf("=");
			space += 1;
			return check.Substring(0, space);
		}

		private bool isSetting(string setting)
		{
			var check = setting 
				.Replace(" ", "")
				.Replace("\t", "");
			var space = setting.IndexOf("=");
			if (check.StartsWith("//"))
				return false;
			if (space == -1)
				return false;
			return check.StartsWith(DEFAULT_LANGUAGE_SETTING);
		}

		private void readConfiguration()
		{
			var file = getConfigFile();
			if (!File.Exists(file))
				return;
			File.ReadAllLines(file).ToList()
				.ForEach(x =>
					{
						parseLine(x);
					});
		}

		private void parseLine(string line)
		{
			var check = line
				.Replace(" ", "")
				.Replace("\t", "");
			var space = line.IndexOf("=");
			if (check.StartsWith("//"))
				return;
			if (space == -1)
				return;
			space += 1;

			if (check.StartsWith(DEFAULT_LANGUAGE_SETTING))
				DefaultLanguage = line.Substring(space, line.Length - space).Trim();
		}

		private string getConfigFile()
		{
			return Path.Combine(_path, Path.Combine(".OpenIDE", "oi.config"));
		}
	}
}
