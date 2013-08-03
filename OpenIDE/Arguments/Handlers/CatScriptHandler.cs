using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class CatScriptHandler : ICommandHandler
	{
		private string _token;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Prints the script to the terminal");
					usage.Add("SCRIPT-NAME", "Script name with optional file extension.");
				return usage;
			}
		}

		public string Command { get { return "cat"; } }

		public CatScriptHandler(string token)
		{
			_token = token;
		}

		public void Execute(string[] arguments)
		{
			if (arguments.Length < 1)
				return;
			var scripts = new List<Script>();
			scripts.AddRange(new ScriptLocator(_token, Environment.CurrentDirectory).GetLocalScripts());
			new ScriptLocator(_token, Environment.CurrentDirectory)
				.GetGlobalScripts()
				.Where(x => scripts.Count(y => x.Name.Equals(y.Name)) == 0).ToList()
				.ForEach(x => scripts.Add(x));
			var script = scripts.FirstOrDefault(x => x.Name.Equals(arguments[0]));
			if (script == null)
				return;
			Console.WriteLine(File.ReadAllText(script.File));
		}
	}
}