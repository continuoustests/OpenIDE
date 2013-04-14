using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using OpenIDE.Core.Language;
using OpenIDE.Core.FileSystem;
using CoreExtensions;

namespace OpenIDE.Arguments.Handlers
{
	public class EventListener : ICommandHandler
	{
		private string _path;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Hooks in to OpenIDE and streams event messages to the console");
				return usage;
			}
		}

		public string Command { get { return "event-listener"; } }

		public EventListener(string path) {
			_path = path;
		}

		public void Execute(string[] arguments)
		{
			var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			query(Path.Combine(root, Path.Combine("EventListener", "OpenIDE.EventListener.exe")), "");
		}

		private void query(string cmd, string arguments)
		{
			try {
				var proc = new Process();
                proc.Query(
                	cmd,
                	arguments,
                	false,
                	_path,
                	(error, s) => Console.WriteLine(s));
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}
	}
}