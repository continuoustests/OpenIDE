using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenIDENet.Core.Config;
using OpenIDENet.Core.Language;

namespace OpenIDENet.Arguments.Handlers
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
				usage.Add("read", "Prints closest configuration");
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
			var path = Directory.GetCurrentDirectory();

			if (arguments[0] == "init")
				initializingConfiguration(path);
			else if (arguments[0] == "read")
				printClosestConfiguration(path);
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

			var config = new Configuration(path, false);
			if (config.ConfigurationFile == null)
			{
				Console.WriteLine("There is no config point at " + path);
				return;
			}

			Console.WriteLine("Writing to " + config.ConfigurationFile);
			Console.WriteLine("\t{0} setting: {1}",
				args.Delete ? "Deleting" : "Updating",
				args.Setting);

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
			var file = Path.Combine(dir, "oi.config");
			File.WriteAllText(file, "");
		}

		private bool isInitialized(string path)
		{
			var file = Path.Combine(Path.Combine(path, ".OpenIDE"), "oi.config");
			return File.Exists(file);
		}

		private void printClosestConfiguration(string path)
		{
			var file = new Configuration(path, true).ConfigurationFile;
			if (file == null)
				return;
			Console.WriteLine("Configuration file: {0}", file);
			Console.WriteLine("");
			File.ReadAllLines(file).ToList()
				.ForEach(x =>
					{
						Console.WriteLine("\t" + x);
					});
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
