using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using CoreExtensions;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Profiles;

namespace OpenIDE.Core.RScripts
{
	public class ReactiveScript
	{
		private bool _isFaulted;
		private string _name;
		private string _file;
		private string _keyPath;
		private bool _dispatchErrors = false;
		private List<string> _events = new List<string>();
		private string _localProfileName;
		private string _globalProfileName;
		private Action<string> _dispatch;

		public string Name {
			get {
				if (_name == null)
					_name = Path.GetFileNameWithoutExtension(_file);
				return _name;
			}
		}

		public bool IsFaulted { get { return _isFaulted; } }

		public string File { get { return _file; } }

		public ReactiveScript(string file, string keyPath, Action<string> dispatch)
		{
			construct(file, keyPath, dispatch, false);
		}

		public ReactiveScript(string file, string keyPath, Action<string> dispatch, bool dispatchErrors)
		{
			construct(file, keyPath, dispatch, dispatchErrors);
		}

		private void construct(string file, string keyPath, Action<string> dispatch, bool dispatchErrors)
		{
			_file = file;
			_keyPath = keyPath;
			_dispatch = dispatch;
			_dispatchErrors = dispatchErrors;
			var profiles = new ProfileLocator(_keyPath);
			_globalProfileName = profiles.GetActiveGlobalProfile();
			_localProfileName = profiles.GetActiveLocalProfile();
			getEvents();
		}

		public bool ReactsTo(string @event)
		{
			if (_isFaulted)
				return false;
			foreach (var reactEvent in _events) {
				if (wildcardmatch(@event, reactEvent))
					return true;
            }
			return false;
		}

		public void Run(string message)
		{
			if (_isFaulted)
				return;
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
			var originalMessage = message;
            message = "{event} {global-profile} {local-profile}";
            Logger.Write("Running: " + _file + " " + message);
            ThreadPool.QueueUserWorkItem((task) => {
	            try
	            {
					var process = new Process();
					process.SetLogger((logMsg) => Logger.Write(logMsg));
	            	var msg = task.ToString();
	            	process.Query(
	            		_file,
	            		msg,
	            		false,
	            		_keyPath,
	            		(error, m) => {
	            			if (m == null)
	            				return;
	            			var cmdText = "command|";
	            			var eventText = "event|";
	            			if (error) {
	            				if (_dispatchErrors)
	            					internalDispatch("rscript-" + Name + " error|" + m);
	            				Logger.Write("rscript-" + Name + " produced an error:");
	            				Logger.Write("rscript-" + Name + "-" + m);
	            			} else {
	            				if (m.StartsWith(cmdText))
	            					internalDispatch(m.Substring(cmdText.Length, m.Length - cmdText.Length));
	            				else if (m.StartsWith(eventText))
	            					internalDispatch(m.Substring(eventText.Length, m.Length - eventText.Length));
	            				else
	            					internalDispatch("rscript-" + Name + " " + m);
	            			}
	            		},
	            		new[] {
							new KeyValuePair<string,string>("{event}", "\"" + originalMessage + "\""),
							new KeyValuePair<string,string>("{global-profile}", "\"" + _globalProfileName + "\""),
							new KeyValuePair<string,string>("{local-profile}", "\"" + _localProfileName + "\"")
						});
	            }
				catch (Exception ex)
				{
					internalDispatch("rscript-" + Name + " " + ex.ToString());
					Logger.Write(ex.ToString());
				}
			}, message);
		}

		private void getEvents()
		{
			_isFaulted = false;
			_events.Clear();
			try {
				new Process()
					.Query(
						_file,
						"reactive-script-reacts-to",
						false,
						_keyPath,
						(error, m) => {
							if (error) {
								_isFaulted = true;
								if (_dispatchErrors)
		            				internalDispatch("rscript-" + Name + " error|" + m);
								Logger.Write(
									"Failed running reactive script with reactive-script-reacts-to: " +
									_file);
								Logger.Write(m);
								return;
							}
							if (m.Length > 0) {
								var expression = m; //m.Trim(new[] {'\"'});
								_events.Add(expression);
								Logger.Write(_file + " reacts to: " + expression);
							}
						});
			} catch (Exception ex) {
				_isFaulted = true;
				internalDispatch("rscript-" + Name + " error|Could not read reacts to information");
				Logger.Write(ex);
			}
		}
		
		private bool wildcardmatch(string str, string pattern)
		{
            return new RScriptMatcher(pattern).Match(str);
		}

		private void internalDispatch(string message)
		{
			// Do this in a separate thread so we don't get deadlocks on _scripts
			ThreadPool.QueueUserWorkItem((m) => {
				_dispatch(m.ToString());
			}, message);
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
