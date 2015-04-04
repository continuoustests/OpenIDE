using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using CoreExtensions;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Language;

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
					"Hooks into OpenIDE and streams event messages to the console");
				usage.Add("[FILTER]", "Supports *something*");
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
			var eventListener = Path.Combine(root, Path.Combine("EventListener", "OpenIDE.EventListener.exe"));
			string filter = null;
			if (arguments.Length == 1)
				filter = arguments[0];
			query(eventListener, "", filter, (m) => Console.WriteLine(m));
		}

		private Regex _matcher;
		private void query(string cmd, string arguments, string filter, Action<string> onMatch)
		{
			if (filter != null) {
				_matcher = new Regex(
					"^" + Regex.Escape(filter)
						.Replace( "\\*", ".*" )
						.Replace( "\\?", "." ) + "$");
			}
			try {
				var proc = new Process();
                proc.Query(
                	cmd,
                	arguments,
                	false,
                	_path,
                	(error, s) => {
            			if (error || filter == null || match(s))
            				onMatch(s);
            		});
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}

		private bool match(string line) {
			return _matcher.IsMatch(line);
		}
	}
}
