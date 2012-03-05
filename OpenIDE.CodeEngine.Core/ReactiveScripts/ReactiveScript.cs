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
			_file = file;
			_keyPath = keyPath;
			getEvents();
		}

		public bool ReactsTo(string @event)
		{
			return _events.Count(x => x.StartsWith(@event)) == 0;
		}

		public void Run(string message)
		{
			var process = new Process();
			process.Run(_file, message, false, _keyPath);
		}

		private void getEvents()
		{
			_events.Clear();
			_events.AddRange(
				new Process()
					.Query(_file, "reactive-script-reacts-to", false, _keyPath)
					.Where(x => x.Length > 0));
		}
	}
}
