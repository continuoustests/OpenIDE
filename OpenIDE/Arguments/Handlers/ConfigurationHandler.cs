using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OpenIDE.Core.Config;
using OpenIDE.Core.Language;
using OpenIDE.Core.Profiles;

namespace OpenIDE.Arguments.Handlers
{
	class ConfigurationHandler : ICommandHandler
	{
		private PluginLocator _pluginLocator;
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Writes a configuration setting in the current path (configs " +
					"are only read from root folder)");
				usage.Add("list", "List available configuration options (*.oicfgoptions)");
				usage.Add("init", "Initializes a configuration point");
				var read = usage.Add("read", "Prints closest configuration");
				read.Add("cfgfile", "Location of nearest configuration file");
				read.Add("cfgpoint", "Location of nearest configuration point");
				read.Add("rootpoint", "Location of current root location");
				read.Add("SETTING_NAME", "The name of the setting to print the value of. Supports wildcard match (global fallback)");
				read.Add("[--global]", "Forces configuration command to be directed towards global config");
				read.Add("[-g]", "Short version of --global");
				var setting = usage.Add("SETTING", "The statement to write to the config");
				setting.Add("[--global]", "Forces configuration command to be directed towards global config");
				setting.Add("[-g]", "Short version of --global");
				setting.Add("[--delete]", "Removes configuration setting");
				setting.Add("[-d]", "Short version of --delete");
				return usage;
			}
		}

		public string Command { get { return "configure"; } }

		public ConfigurationHandler(PluginLocator locator) {
			_pluginLocator = locator;
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length < 1)
			{
				Console.WriteLine("Invalid number of arguments. Usage " + Command + " SETTING");
				return;
			}
			var path = Environment.CurrentDirectory;

			if (arguments[0] == "init")
				initializingConfiguration(path);
			else if (arguments[0] == "list")
				printConfigurationOptions(path);
			else if (arguments[0] == "read")
				printClosestConfiguration(path, arguments);
			else
				updateConfiguration(path, arguments);
		}

		private void printConfigurationOptions(string path)
		{
			var file = new Configuration(path, true).ConfigurationFile;
			var paths = new List<string>();
			paths.Add(Path.GetDirectoryName(file));
			foreach (var plugin in _pluginLocator.Locate())
				paths.Add(plugin.GetPluginDir());
			var reader = new ConfigOptionsReader(paths.ToArray());
			reader.Parse();
			foreach (var line in reader.Options)
				Console.WriteLine(line);
		}

		private void updateConfiguration(string path, string[] arguments)
		{
			var args = parseArguments(arguments);
			if (args == null)
				return;

			if (args.Global)
				path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			if (!Configuration.IsConfigured(path))
			{
				Console.WriteLine("There is no config point at " + path);
				return;
			}

			var config = new Configuration(path, false);
			if (args.Delete)
				config.Delete(args.Settings[0]);
			else
				config.Write(args.Settings[0]);
		}

		private void initializingConfiguration(string path)
		{
			if (isInitialized(path))
				return;
			var dir = Path.Combine(path, ".OpenIDE");
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			File.WriteAllText(Path.Combine(dir, "oi.config"), "");
		}

		private bool isInitialized(string path)
		{
			var file = Path.Combine(path, ".OpenIDE");
			return Directory.Exists(file);
		}

		private void printClosestConfiguration(string path, string[] arguments)
		{
			var args = parseArguments(arguments);
			if (args == null)
				return;
			if (args.Global)
				path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var file = new Configuration(path, true).ConfigurationFile;
			if (!File.Exists(file))
				return;
			string pattern =  null;
			var wildcard = false;
			if (args.Settings.Length == 2)
				pattern = args.Settings[1];
			if (pattern == null) {
				Console.WriteLine("Configuration file: {0}", file);
				Console.WriteLine("");
				File.ReadAllLines(file).ToList()
					.ForEach(x => {
							Console.WriteLine("\t" + x);
						});
				return;
			}
			if (pattern == "cfgpoint") {
				Console.Write(Path.GetDirectoryName(file));
				return;
			}
			if (pattern == "rootpoint") {
				Console.Write(Path.GetDirectoryName(new ProfileLocator(path).GetLocalProfilesRoot()));
				return;
			}
			if (pattern == "cfgfile") {
				Console.Write(file);
				return;
			}
			if (pattern.EndsWith("*")) {
				wildcard = true;
				pattern = pattern.Substring(0, pattern.Length - 1);
			}
			if (wildcard) {
				getSettingsStartingWithWildcard(File.ReadAllLines(file), pattern)
					.ForEach(x => Console.WriteLine(x));
			} else {
				var value = getSetting(File.ReadAllLines(file), pattern);
				if (value == null) {
					var global = 
						new Configuration(
							Path.GetDirectoryName(
								Assembly.GetExecutingAssembly().Location),
							false)
						.ConfigurationFile;
					if (File.Exists(global)) {
						value = getSetting(File.ReadAllLines(global), pattern);
					}
				}
				Console.Write(value);
			}
		}

		private List<string> getSettingsStartingWithWildcard(IEnumerable<string> lines, string pattern) {
			return lines.ToList()
				.Where(x => {
						var s = x.Replace(" ", "").Replace("\t", "");
						return x.StartsWith(pattern);
					})
				.Select(x => x.Trim(new[] { ' ', '\t' }))
				.ToList();
		}

		private string getSetting(IEnumerable<string> lines, string pattern) {
			var item = lines.ToList()
				.FirstOrDefault(x => {
						var s = x.Replace(" ", "").Replace("\t", "");
						return x.StartsWith(pattern + "=");
					});
			if (item == null)
				return null;

			var equals = item.IndexOf("=") + 1;
			return item.Substring(equals, item.Length - equals);
		}
		
		private CommandArguments parseArguments(string[] arguments)
		{
			var settings = arguments.Where(x => !x.StartsWith("-")).ToArray();
			if (settings.Length == 0)
			{
				Console.WriteLine("error|No argument provided");
				return null;
			}
			return new CommandArguments()
				{
					Settings = settings,
					Global = arguments.Contains("--global") || arguments.Contains("-g"),
					Delete = arguments.Contains("--delete") || arguments.Contains("-d")
				};
		}

		class CommandArguments
		{
			public string[] Settings { get; set; }
			public bool Global { get; set; }
			public bool Delete { get; set; }
		}
	}
}
