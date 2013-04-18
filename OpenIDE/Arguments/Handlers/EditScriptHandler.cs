using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Language;

namespace OpenIDE.Arguments.Handlers
{
	class EditScriptHandler : ICommandHandler
	{
		private string _token;
		private Action<string> _dispatch;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Opens a script for edit");
					usage.Add("SCRIPT-NAME", "Script name. Local are picked over global");
					return usage;
			}
		}
	
		public string Command { get { return "edit"; } }
	
		public EditScriptHandler(string token, Action<string> dispatch)
		{
			_token = token;
			_dispatch = dispatch;
		}

		public void Execute(string[] arguments)
		{
			var scripts = new List<Script>();
			scripts.AddRange(new ScriptLocator(_token, Environment.CurrentDirectory).GetLocalScripts());
			new ScriptLocator(_token, Environment.CurrentDirectory)
				.GetGlobalScripts()
				.Where(x => scripts.Count(y => x.Name.Equals(y.Name)) == 0).ToList()
				.ForEach(x => scripts.Add(x));
			var script = scripts.FirstOrDefault(x => x.Name.Equals(arguments[0]));
			if (script == null || arguments.Length < 1)
				return;
			_dispatch(string.Format("command|editor goto \"{0}|0|0\"", script.File));
		}
	}
}
