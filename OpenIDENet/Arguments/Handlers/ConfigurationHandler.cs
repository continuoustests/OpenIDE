using System;
using OpenIDENet.Core.Config;
using OpenIDENet.Core.Language;

namespace OpenIDENet.Arguments.Handlers
{
	class ConfigurationHandler : ICommandHandler
	{
		private string _path;

		public ConfigurationHandler(string path)
		{
			_path = path;
		}

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Writes a configuration setting in the current path (configs " +
					"are only read from root folder)");
				usage.Add("SETTING", "The statement to write to the config");
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
			new Configuration(_path)
				.Write(arguments[0]);
		}
	}
}
