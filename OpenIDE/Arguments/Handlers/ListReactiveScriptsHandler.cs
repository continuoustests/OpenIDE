using System;
using System.Linq;
using OpenIDE.Core.Language;
using OpenIDE.Core.FileSystem;

namespace OpenIDE.Arguments.Handlers
{
	public class ListReactiveScriptsHandler : ICommandHandler
	{
		private string _token;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Lists reactive scripts");
					return usage;
			}
		}
	
		public string Command { get { return "list"; } }

		public ListReactiveScriptsHandler(string token)
		{
			_token = token;
		}

		public void Execute(string[] arguments)
		{
			var globalScripts = new ReactiveScriptLocator(Environment.CurrentDirectory).GetGlobalScripts();
			if (globalScripts.Length > 0)
				Console.WriteLine("Global scripts:");
			foreach (var script in globalScripts)
				Console.WriteLine("\t" + script.Name);
			
			if (globalScripts.Length > 0)
				Console.WriteLine("");
				
			var localScripts = new ReactiveScriptLocator(Environment.CurrentDirectory).GetLocalScripts();
			if (localScripts.Length > 0)
				Console.WriteLine("Local scripts:");
			foreach (var script in localScripts)
				Console.WriteLine("\t" + script.Name);
		}
	}
}