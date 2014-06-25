using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using OpenIDE.Core.Profiles;
using OpenIDE.Core.Logging;
using OpenIDE.Core.Requests;
using CoreExtensions;

namespace OpenIDE.Core.FileSystem
{
	public class Script
	{
		private string _file;
		private string _token;
		private string _workingDirectory;
		private string _localProfileName;
		private string _globalProfileName;
		private bool _isRunning;
		private Action<string> _writer = (msg) => {};
		private Action<string> _usageDispatcher = (msg) => {};

		public IEnumerable<BaseCommandHandlerParameter> Usages { get { return getUsages(); } }

		public string File { get { return _file; } }
		public string Name { get; private set; }
		public string Description { get; private set; }

		public Script(string token, string workingDirectory, string file)
		{
			_file = file;
			_token = token;
			Name = Path.GetFileNameWithoutExtension(file);
			Description = "";
			_workingDirectory = workingDirectory;
			var profiles = new ProfileLocator(_token);
			_globalProfileName = profiles.GetActiveGlobalProfile();
			_localProfileName = profiles.GetActiveLocalProfile();
		}

		public Script SetUsageDispatcher(Action<string> usageDispatcher)
		{
			_usageDispatcher = usageDispatcher;
			return this;
		}

		public void Write(string message)
		{
			_writer(message);
		}

		public void Run(string arguments, Action<string> onLine)
		{
			var process = new Process();
			_isRunning = true;
			var stdinForwarder = new Thread(() => {
				try {
					while (_isRunning) {
						var line = Console.ReadLine();
						process.Write(line);
					}
				} catch {
				}
			});
			stdinForwarder.Start();

			Logger.Write("Running script {0} with {1}", _file, arguments);
			arguments = "{global-profile} {local-profile} " + arguments;
			run(
				arguments,
				(m) => {
					var requestRunner = new RequestRunner(_token);
					if (requestRunner.IsRequest(m)) {
						ThreadPool.QueueUserWorkItem((msg) => {
							var response = requestRunner.Request(msg.ToString());
	    					foreach (var content in response)
	    						process.Write(content);
	    				},
	    				m);
					} else {
						onLine(m);
					}
				},
				new[] {
						new KeyValuePair<string,string>("{global-profile}", "\"" + _globalProfileName + "\""),
						new KeyValuePair<string,string>("{local-profile}", "\"" + _localProfileName + "\"")
				},
				process);
			_isRunning = false;
			stdinForwarder.Abort();
			Logger.Write("Running script completed {0}", _file);
		}

		private IEnumerable<BaseCommandHandlerParameter> getUsages()
		{
			var usage = getUsage();
			usage = stripDescription(usage);
			return new UsageParser(usage).Parse().ToList();
		}

		private string stripDescription(string usage)
		{
			var end = usage.IndexOf("|");
			if (end == -1)
			{
				Description = usage.Trim(new[] { '\"' });
				return "";
			}
			Description = usage.Substring(0, end).Trim(new[] { '\"' });
			return usage.Substring(
				end + 1,
				usage.Length - (end + 1));
		}
		
		private string getUsage()
		{
			return ToSingleLine("get-command-definitions", _usageDispatcher);
		}

		private string ToSingleLine(string arguments)
		{
			return ToSingleLine(arguments, (s) => {});
		}

		private string ToSingleLine(string arguments, Action<string> onLine)
		{
			var sb = new StringBuilder();
			run(arguments,
				(line) => {
					onLine(line);
					sb.Append(line.Replace(Environment.NewLine, " "));
				},
				new KeyValuePair<string,string>[] {});
			return sb.ToString();
		}

		private string run(string arguments, Action<string> onLine,
						 IEnumerable<KeyValuePair<string,string>> replacements)
		{
			return run(arguments, onLine, replacements, null);
		}

		private string run(string arguments, Action<string> onLine,
						 IEnumerable<KeyValuePair<string,string>> replacements,
						 Process proc)
		{
			var cmd = _file;
			var finalReplacements = new List<KeyValuePair<string,string>>();
			finalReplacements.Add(new KeyValuePair<string,string>("{run-location}", "\"" + _workingDirectory + "\""));
			finalReplacements.AddRange(replacements);
            arguments = "{run-location} " + arguments;
            if (proc == null)
				proc = new Process();
			_writer = (msg) => { 
				try {
					Logger.Write("Writing to the process " + msg);
					proc.Write(msg);
				} catch (Exception ex) {
					Logger.Write(ex);
				}
			};
			
			var realArguments = arguments;
			proc
				.Query(
					cmd,
					arguments,
					false,
					_token,
					(error, line) => {
							if (error && !line.StartsWith("error|")) {
								onLine("error|" + line);
								Logger.Write(line);
							} else
								onLine(line);
						},
					finalReplacements.ToArray(),
					(args) => realArguments = args);
			_writer = (msg) => {};
			return string.Format("\"{0}\" {1}", Name, realArguments);
		}
	}
}
