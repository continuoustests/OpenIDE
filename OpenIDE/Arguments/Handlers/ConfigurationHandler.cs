using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenIDE.Core.Config;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class ConfigurationHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Writes a configuration setting in the current path (configs " +
					"are only read from root folder)");
				usage.Add("init", "Initializes a configuration point");
				var read = usage.Add("read", "Prints closest configuration");
				read.Add("cfgpoint", "Location of nearest configuration file");
				read.Add("SETTING_NAME", "The name of the setting to print the value of. If name ends with * it will print all matching settings");
				var setting = usage.Add("SETTING", "The statement to write to the config");
				setting.Add("[--global]", "Forces configuration command to be directed towards global config");
				setting.Add("[-g]", "Short version of --global");
				setting.Add("[--delete]", "Removes configuration setting");
				setting.Add("[-d]", "Short version of --delete");
				return usage;
			}
		}

		public string Command { get { return "configure"; } }

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
			else if (arguments[0] == "read")
				printClosestConfiguration(path, arguments);
			else
				updateConfiguration(path, arguments);
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
			/*Console.WriteLine("Writing to " + config.ConfigurationFile);
			Console.WriteLine("\t{0} setting: {1}",
				args.Delete ? "Deleting" : "Updating",
				args.Setting);*/

			if (args.Delete)
				config.Delete(args.Setting);
			else
				config.Write(args.Setting);
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
			var file = new Configuration(path, true).ConfigurationFile;
			if (!File.Exists(file))
				return;
			string pattern =  null;
			var wildcard = false;
			if (arguments.Length == 2)
				pattern = arguments[1];
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
				Console.Write(file);
				return;
			}
			if (pattern.EndsWith("*")) {
				wildcard = true;
				pattern = pattern.Substring(0, pattern.Length - 1);
			}
			if (wildcard) {
				File.ReadAllLines(file).ToList()
					.ForEach(x => {
						var s = x.Replace(" ", "").Replace("\t", "");
						if (x.StartsWith(pattern)) {
							Console.WriteLine(x.Trim(new[] { ' ', '\t' }));
						}
					});
			} else {
				File.ReadAllLines(file).ToList()
					.ForEach(x => {
						var s = x.Replace(" ", "").Replace("\t", "");
						if (x.StartsWith(pattern + "=")) {
							var equals = x.IndexOf("=") + 1;
							Console.Write(x.Substring(equals, x.Length - equals));
							return;
						}
					});
			}
		}
		
		private CommandArguments parseArguments(string[] arguments)
		{
			var setting = arguments.FirstOrDefault(x => !x.StartsWith("-"));
			if (setting == null)
			{
				Console.WriteLine("error|No setting argument provided");
				return null;
			}
			return new CommandArguments()
				{
					Setting = setting,
					Global = arguments.Contains("--global") || arguments.Contains("-g"),
					Delete = arguments.Contains("--delete") || arguments.Contains("-d")
				};
		}

		class CommandArguments
		{
			public string Setting { get; set; }
			public bool Global { get; set; }
			public bool Delete { get; set; }
		}
	}
}
