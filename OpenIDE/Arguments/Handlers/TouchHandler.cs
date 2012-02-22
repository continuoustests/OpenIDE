using System;
using System.Collections.Generic;
using System.IO;
using OpenIDE.Core.Language;
using OpenIDE.FileSystem;

namespace OpenIDE.Arguments.Handlers
{
	class TouchHandler : ICommandHandler
	{
		private Action<string> _dispatch;

		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.FileCommand,
					Command,
					"Creates a new file together with required directories and opens it in the editor");
				usage.Add("FILE", "Path to the file you want to create");
				return usage;
			}
		}

		public string Command { get { return "touch"; } }

		public TouchHandler(Action<string> dispatch)
		{
			_dispatch = dispatch;
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length != 1)
				return;
			var file = Path.GetFullPath(arguments[0]);
			PathExtensions.CreateDirectories(file);
			File.WriteAllText(file, "");
			_dispatch("editor goto \"" + file + "|0|0\"");
		}
	}
}
