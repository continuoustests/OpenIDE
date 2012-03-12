using System;
using System.Linq;
using OpenIDE.Core.Language;
using OpenIDE.Core.RScripts;

namespace OpenIDE.Arguments.Handlers
{
	class EditReactiveScriptHandler : ICommandHandler
	{
		private Action<string> _dispatch;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Opens an existing reactive script for editor");
					usage.Add("SCRIPT-NAME", "Reactive script name. Local are picked over global");
					return usage;
			}
		}
	
		public string Command { get { return "rscript-edit"; } }
		
		public EditReactiveScriptHandler(Action<string> dispatch)
		{
			_dispatch = dispatch;
		}
	
		public void Execute(string[] arguments)
		{
			var scripts = new ReactiveScriptReader().Read(Environment.CurrentDirectory);
			var script = scripts.FirstOrDefault(x => x.Name.Equals(arguments[0]));
			if (script == null || arguments.Length < 1)
				return;
			_dispatch(string.Format("editor goto \"{0}|0|0\"", script.File));
		}
	}
}
