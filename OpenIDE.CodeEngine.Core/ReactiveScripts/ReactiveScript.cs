using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using CoreExtensions;

namespace OpenIDE.CodeEngine.Core.ReactiveScripts
{
	class ReactiveScript
	{
		private string _file;
		private string _keyPath;
		private List<string> _events = new List<string>();	

		public ReactiveScript(string file, string keyPath)
		{
			Logging.Logger.Write("Adding reactive script {0} with path {1}", file, keyPath);
			_file = file;
			_keyPath = keyPath;
			getEvents();
		}

		public bool ReactsTo(string @event)
		{
			foreach (var reactEvent in _events)
				if (@event.StartsWith(reactEvent))
					return true;
			return false;
		}

		public void Run(string message)
		{
			if (Environment.OSVersion.Platform != PlatformID.Unix &&
				Environment.OSVersion.Platform != PlatformID.MacOSX)
			{
				message = message
						  	.Replace("\"", "^\"")
							.Replace(" ", "^ ")
							.Replace("|", "^|")
							.Replace("%", "^&")
							.Replace("&", "^&")
							.Replace("<", "^<")
							.Replace(">", "^>");
			}
			message = "\"" + message + "\"";
			Logging.Logger.Write("running {0} with {1}", _file, message);
			var process = new Process();
			process.Run(_file, message, false, _keyPath);
		}

		private void getEvents()
		{
			_events.Clear();
			_events.AddRange(
				new Process()
					.Query(_file, "reactive-script-reacts-to", false, _keyPath)
					.Where(x => x.Length > 0)
					.Select(x => x.Trim(new[] {'\"'})));
			_events
				.ForEach(x => Logging.Logger.Write("\tReacting to " + x));
		}
	}
}
