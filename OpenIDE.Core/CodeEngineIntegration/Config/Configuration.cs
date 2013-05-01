using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.CommandBuilding;

namespace OpenIDE.Core.Config
{
	public class ConfigurationSetting
	{
		public string Key { get; private set; }
		public string Value { get; private set; }

		public ConfigurationSetting(string key, string value) {
			Key = key;
			Value = value;
		}

		public string[] SplitBy(string delimiter) {
			if (Value == null)
				return new string[] {};
			return Value.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
		}
	}

	public class Configuration
	{
		private const string DEFAULT_EDITOR_SETTING = "default.editor";
		private const string DEFAULT_LANGUAGE_SETTING = "default.language";
		private const string ENABLED_LANGUAGES_SETTING = "enabled.languages";
		private string[] _operators = new[] { "+=","-=","=" };
		private string _path;
		private bool _allowGlobal = false;
		private List<string> _editorSettings = new List<string>();

		public string ConfigurationFile { get; private set; }

		public string DefaultEditor { get; private set; }
		public string DefaultLanguage { get; private set; }
		public string[] EnabledLanguages { get; private set; }
		public string[] EditorSettings { get { return _editorSettings.ToArray(); } }

		public Configuration(string path, bool allowGlobal)
		{
			_path = path;
			_allowGlobal = allowGlobal;
			ConfigurationFile = GetConfigFile(_path);
			if (!File.Exists(ConfigurationFile))
				ConfigurationFile = null;
			if (ConfigurationFile == null && _allowGlobal)
				ConfigurationFile = GetConfigFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			readConfiguration();
		}

		public static string GetConfigFile(string path)
		{
			var point = getConfigPoint(path);
			if (point == null)
				return null;
			return Path.Combine(point, "oi.config");
		}
		
		public static string GetConfigPoint(string path)
		{
			return getConfigPoint(path);
		}

		public static bool IsConfigured(string path)
		{
			return getConfigPoint(path) != null;
		}
		
		public ConfigurationSetting[] GetSettingsStartingWith(string setting)
		{
			if (ConfigurationFile == null)
				ConfigurationFile = GetConfigFile(_path);
			if (ConfigurationFile == null)
				return new ConfigurationSetting[] {};
			var settings = new List<ConfigurationSetting>();
			foreach (var rawLine in File.ReadAllLines(ConfigurationFile)) {
				var line = rawLine.Trim(new[] { ' ', '\t' });
				var key = getTag(line);
				if (key.StartsWith(setting)) {
					var value = getValue(line);
					settings.Add(new ConfigurationSetting(key, value));
				}
			}
			return settings.ToArray();
		}

		public ConfigurationSetting Get(string setting)
		{
			if (ConfigurationFile == null)
				ConfigurationFile = GetConfigFile(_path);
			if (ConfigurationFile == null)
				return null;
			ConfigurationSetting cfgSetting = null;
			foreach (var rawLine in File.ReadAllLines(ConfigurationFile)) {
				var line = rawLine.Trim(new[] { ' ', '\t' });
				if (getTag(line) == setting) {
					var value = getValue(line);
					cfgSetting = new ConfigurationSetting(setting, value);
				}
			}
			return cfgSetting;
		}

		public void Write(string setting)
		{
			setting = setting.Trim(new[] { '\t' });
			if (ConfigurationFile == null)
				ConfigurationFile = GetConfigFile(_path);
			if (ConfigurationFile == null)
			{
				Console.WriteLine("Could not find valid configuration point for path " + _path);
				return;
			}
			if (!isSetting(setting))
			{
				Console.WriteLine("Invalid setting: " + setting);
				return;
			}
			if (!Directory.Exists(Path.GetDirectoryName(ConfigurationFile)))
				Directory.CreateDirectory(Path.GetDirectoryName(ConfigurationFile));
			string[] lines = new string[] {};
			if (File.Exists(ConfigurationFile))
				lines = File.ReadAllLines(ConfigurationFile); write(ConfigurationFile, lines, setting);
		}

		public void Delete(string setting)
		{
			if (!File.Exists(ConfigurationFile))
				ConfigurationFile = GetConfigFile(_path);
			if (!File.Exists(ConfigurationFile))
			{
				Console.WriteLine("Could not find valid configuration point for path " + _path);
				return;
			}
			var lines = File.ReadAllLines(ConfigurationFile);
			remove(ConfigurationFile, lines, setting);
		}

		private void write(string file, string[] lines, string setting)
		{
			var settingTag = getTag(setting);
			var settingOperator = getOperator(setting);
			var settingValue = getValue(setting);
			using (var writer = new StreamWriter(file))
			{
				var written = false;
				lines.ToList()
					.ForEach(x => 
						{
							if (getTag(x).Equals(settingTag))
							{
								writeSetting(writer, x, settingTag, settingOperator, settingValue);
								written = true;
							}
							else
								writer.WriteLine(x);
						});
				if (!written)
					writeSetting(writer, "", settingTag, settingOperator, settingValue);
			}
		}

		private void writeSetting(StreamWriter writer, string original, string name, string operatr, string val)
		{
			if (operatr == "+=")
			{
				if (original == "")
					writer.WriteLine(name + "=" + val);
				else
					writer.WriteLine(original + "," + val);
			}
			else if (operatr == "-=")
				removeValueFromList(writer, original, val);
			else
				writer.WriteLine(name + operatr + val);
		}

		private void removeValueFromList(StreamWriter writer, string setting, string val)
		{
			var values = getValuesFromSetting(setting);
			if (values.Length == 0)
				return;
			var edited = getTag(setting) + "=";
			foreach (var vlue in values)
			{
				if (vlue != val)
					edited += vlue + ",";
			}
			if (edited.EndsWith(","))
			{
				edited = edited.Substring(0, edited.Length - 1);
				writer.WriteLine(edited);
			}
		}

		private string[] getValuesFromSetting(string setting)
		{
			if (setting == "")
				return new string[] {};
			var val = getValue(setting);
			return new CommandStringParser(',').Parse(val).ToArray();
		}

		private string getOperator(string setting)
		{
			var operatorPosition = -1;
			foreach (var operatr in _operators)
			{
				operatorPosition = setting.IndexOf(operatr);
				if (operatorPosition != -1)
					return operatr.Trim();
			}
			return null;
		}

		private string getValue(string setting)
		{
			var operatorEnd = getOperatorPosition(setting);
			if (operatorEnd == -1)
				return null;
			operatorEnd += getOperator(setting).Length;
			return setting.Substring(operatorEnd, setting.Length - operatorEnd).Trim();
		}

		private void remove(string file, string[] lines, string setting)
		{
			using (var writer = new StreamWriter(file))
			{
				var written = false;
				lines.ToList()
					.ForEach(x => 
						{
							if (!getTag(x).Equals(setting))
								writer.WriteLine(x);
						});
			}
		}

		private string getTag(string setting)
		{
			var check = setting 
				.Replace(" ", "")
				.Replace("\t", "");
			var operatr = getOperatorPosition(check);
			return check.Substring(0, operatr).Trim();
		}

		private int getOperatorPosition(string setting)
		{
			var operatorPosition = -1;
			foreach (var operatr in _operators)
			{
				operatorPosition = setting.IndexOf(operatr);
				if (operatorPosition != -1)
					return operatorPosition;
			}
			return operatorPosition;
		}

		private bool isSetting(string setting)
		{
			var check = setting 
				.Replace(" ", "")
				.Replace("\t", "");
			if (check.StartsWith("//"))
				return false;
			var operatr = getOperatorPosition(setting);
			if (operatr == -1)
				return false;
			return true;;
		}

		private void readConfiguration()
		{
			if (ConfigurationFile == null)
				return;
			if (!File.Exists(ConfigurationFile))
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

			if (check.StartsWith(DEFAULT_EDITOR_SETTING))
				DefaultEditor = line.Substring(space, line.Length - space).Trim();
			if (check.StartsWith(DEFAULT_LANGUAGE_SETTING))
				DefaultLanguage = line.Substring(space, line.Length - space).Trim();
			if (check.StartsWith(ENABLED_LANGUAGES_SETTING))
			{
				EnabledLanguages = 
					new CommandStringParser(',')
						.Parse(line
								.Substring(space, line.Length - space).Trim()).ToArray();
			}
			if (check.StartsWith("editor.")) {
				_editorSettings.Add(check.Trim(new[] { ' ', '\t' }));
			}
		}

		private static string getConfigPoint(string path)
		{
			var locator = new ProfileLocator(path);
			return locator.GetLocalProfilePath(locator.GetActiveLocalProfile());
		}
	}
}
