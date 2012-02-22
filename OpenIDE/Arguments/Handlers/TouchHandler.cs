using System;
using System.Collections.Generic;
using System.IO;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class TouchHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Creates a new file together with required directories");
				usage.Add("FILE", "Path to the file you want to create");
				return usage;
			}
		}

		public string Command { get { return "touch"; } }

		public void Execute(string[] arguments)
		{
			if (arguments.Length != 1)
				return;
			var file = Path.GetFullPath(arguments[0]);
			createDirectories(file);
			File.WriteAllText(file, "");
		}
		
		private void createDirectories(string file)
		{
			var unexisting = new List<string>();
			var dir = Path.GetDirectoryName(file);
			while (!Directory.Exists(dir))
			{
				unexisting.Insert(0, Path.GetFileName(dir));
				dir = Path.GetDirectoryName(dir);
				if (dir == null)
					break;
			}
			unexisting.ForEach(x => {
				dir = Path.Combine(dir, x);
				Directory.CreateDirectory(dir);
			});
		}
	}
}
