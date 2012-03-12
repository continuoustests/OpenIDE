using System;
using System.IO;
using System.Linq;
using OpenIDE.Core.Language;
using OpenIDE.Core.RScripts;

namespace OpenIDE.Arguments.Handlers
{
	class DeleteReactiveScriptHandler : ICommandHandler
	{
		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Deletes a reactive script");
					usage.Add("SCRIPT-NAME", "Reactive script name. Local are picked over global");
					return usage;
			}
		}
	
		public string Command { get { return "rscript-delete"; } }
	
		public void Execute(string[] arguments)
		{
			var scripts = new ReactiveScriptReader().Read(Environment.CurrentDirectory);
			var script = scripts.FirstOrDefault(x => x.Name.Equals(arguments[0]));
			if (script == null || arguments.Length < 1)
				return;
			File.Delete(script.File);
		}
	}
}
