using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CoreExtensions;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.RScripts
{
	public class ReactiveScript
	{
		private string _file;
		private string _keyPath;
		private List<string> _events = new List<string>();	

		public string Name { get { return Path.GetFileNameWithoutExtension(_file); } }
		public string File { get { return _file; } }

		public ReactiveScript(string file, string keyPath)
		{
			_file = file;
			_keyPath = keyPath;
			getEvents();
		}

		public bool ReactsTo(string @event)
		{
			foreach (var reactEvent in _events) {
				if (wildcardmatch(@event, reactEvent))
					return true;
            }
			return false;
		}

		public void Run(string message)
		{
			if (Environment.OSVersion.Platform != PlatformID.Unix &&
				Environment.OSVersion.Platform != PlatformID.MacOSX)
			{
				message = message
							.Replace(" ", "^ ")
							.Replace("|", "^|")
							.Replace("%", "^&")
							.Replace("&", "^&")
							.Replace("<", "^<")
							.Replace(">", "^>");
			}
            message = "\"" + message + "\"";
			var process = new Process();
            Logger.Write("Running: " + _file + " " + message);
            try
            {
            	process.Run(_file, message, false, _keyPath);
            }
			catch (Exception ex)
			{
				Logger.Write(ex.ToString());
			}
		}

		private void getEvents()
		{
			_events.Clear();
			new Process()
				.Query(
					_file,
					"reactive-script-reacts-to",
					false,
					_keyPath,
					(error, m) => {
						if (error) {
							Logger.Write(
								"Failed running reactive script with reactive-script-reacts-to: " +
								_file);
							Logger.Write(m);
							return;
						}
						if (m.Length > 0) 
							_events.Add(m.Trim(new[] {'\"'}));
					});
		}
		
		private bool wildcardmatch(string str, string pattern)
		{
            return new RScriptMatcher(pattern).Match(str);
		}
	}

    public class RScriptMatcher
    {
        private Regex _matcher;

        public RScriptMatcher(string pattern)
        {
             _matcher = new Regex(
				"^" + Regex.Escape(pattern)
					.Replace( "\\*", ".*" )
					.Replace( "\\?", "." ) + "$");
        }

        public bool Match(string text)
        {
            return _matcher.IsMatch(text);
        }
    }
}
