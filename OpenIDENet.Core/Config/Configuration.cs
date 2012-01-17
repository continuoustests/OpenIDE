using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenIDENet.Core.Config
{
	public class Configuration
	{
		private const string DEFAULT_LANGUAGE_SETTING = "default-language=";

		private string _path;
		private bool _allowGlobal = false;

		public string ConfigurationFile { get; private set; }

		public string DefaultLanguage { get; private set; }

		public Configuration(string path, bool allowGlobal)
		{
			_path = path;
			_allowGlobal = allowGlobal;
			ConfigurationFile = getConfigFile(_path);
			if (ConfigurationFile == null && _allowGlobal)
				ConfigurationFile = getConfigFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			readConfiguration();
		}

		public static string GetConfigFile(string path)
		{
			return getConfigFile(path);
		}

		public void Write(string setting)
		{
			if (!isSetting(setting))
			{
				Console.WriteLine("Invalid setting: " + setting);
				return;
			}
			if (!Directory.Exists(Path.GetDirectoryName(ConfigurationFile)))
				Directory.CreateDirectory(Path.GetDirectoryName(ConfigurationFile));
			string[] lines = new string[] {};
			if (File.Exists(ConfigurationFile))
				lines = File.ReadAllLines(ConfigurationFile);
			write(ConfigurationFile, lines, setting);
		}

		public void Delete(string setting)
		{
			if (!File.Exists(ConfigurationFile))
				return;
			var lines = File.ReadAllLines(ConfigurationFile);
			remove(ConfigurationFile, lines, setting + "=");
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

		private void remove(string file, string[] lines, string setting)
		{
			using (var writer = new StreamWriter(file))
			{
				var written = false;
				lines.ToList()
					.ForEach(x => 
						{
							if (getTag(x).Equals(setting))
								written = true;
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
			if (ConfigurationFile == null)
				return;
			File.ReadAllLines(ConfigurationFile).ToList()
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

		private static string getConfigFile(string path)
		{
			if (path == null)
				return null;
			var file = Path.Combine(path, Path.Combine(".OpenIDE", "oi.config"));
			if (!File.Exists(file))
			{
				try {
					return getConfigFile(Path.GetDirectoryName(path));
				} catch {
					return null;
				}
			}
			return file;
		}
	}
}
