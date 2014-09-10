using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using OpenIDE.Core.CommandBuilding;
using OpenIDE.Core.Logging;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Integration;
using CoreExtensions;

namespace OpenIDE.Core.Language
{
	public class LanguagePlugin
	{
		private Thread _pluginLoop;
		private string _path;
		private Action<string> _dispatch;
		private object _processLock = new object();
		private Process _process;
		private bool _isQuerying = false;
		private Action<string> _responseDispatcher = (m) => {};
		private string _crawlFileTypes = null;
		private static Random _rnd = new Random();

		public string FullPath { get { return _path; } }

		public LanguagePlugin(
			string path,
			Action<string> dispatch)
		{
			_path = path;
			_dispatch = dispatch;
		}

		public string GetPluginDir()
		{
			return 
				Path.Combine(Path.GetDirectoryName(_path),
				Path.GetFileNameWithoutExtension(_path) + "-files");
		}

		public string GetLanguage()
		{
			return Path.GetFileNameWithoutExtension(_path);
		}

        public void Initialize(string keyPath)
        {
        	Logger.Write("Initializig " + _path + " with " + keyPath);
        	var initialized = false;
			_pluginLoop = new Thread(() => {
        			var proc = new Process();
        			run(
        				proc,
        				string.Format("initialize \"{0}\"", keyPath),
        				(line) => {
        						if (line == "initialized") {
        							_process = proc;
        							initialized = true;
        						}
        						if (line == "end-of-conversation") {
        							_isQuerying = false;
        							return;
        						}
								_dispatch("event|language-plugin-outout " + line);
        						_responseDispatcher(line);
	        					_dispatch(line);
	        				});
        			initialized = true;
        		});
			_pluginLoop.Start();
        	while (!initialized)
        		Thread.Sleep(10);
			_pluginLoop = null;
        }

        public void Shutdown()
        {
            run("shutdown");
			Logger.Write("Shutdown sent");
			if (_pluginLoop != null) {
				Logger.Write("Joining thread");
				_pluginLoop.Join();
			}
			if (_process != null) {
				if (!_process.HasExited) {
					Logger.Write("Killing process");
					_process.Kill();
				}
			}
        }

		public IEnumerable<BaseCommandHandlerParameter> GetUsages()
		{
			return getUsages();
		}

		public string GetCrawlFileTypes()
		{
			if (_crawlFileTypes == null) {
				_crawlFileTypes = ToSingleLine("crawl-file-types");
			}
			return _crawlFileTypes;
		}

		public void Crawl(IEnumerable<string> filesAndFolders, Action<string> onLineReceived)
		{
			var toCrawl = filesAndFolders.ToArray();
			var filename = "openide-crawl-"+ DateTime.Now.Ticks.ToString() + "-" + _rnd.Next(1,10000).ToString();
			var file = Path.Combine(FS.GetTempPath(), filename);

			using (var stream = new FileStream(file, FileMode.Create)) {
				using (var writer = new StreamWriter(stream)) {
					foreach (var line in toCrawl) {
						writer.WriteLine(line);
					}
				}
			}
			run(string.Format("crawl-source \"{0}\"", file), onLineReceived);
			File.Delete(file);
		}

		public SignatureLocation SignatureFromPosition(FilePosition position)
		{
			try {
				var lines = new List<string>();
				run(string.Format(
						"signature-from-position \"{0}\"",
						position.ToCommand()),
				    (m) => lines.Add(m));
				if (lines.Count != 4)
					return null;
				return new SignatureLocation(
					lines[0],
					lines[1],
					new Position(lines[2]),
					new Position(lines[3]));
			} catch {
				return null;
			}
		}

		public string[] RetrieveMembersFromSignature(string signature)
		{
			var lines = new List<string>();
			run("members-from-signature \"" + signature + "\"", (m) => lines.Add(m));
			return lines.ToArray();
		}

        public string[] Query(string[] arguments)
		{
			var sb = new StringBuilder();
			arguments.ToList()
				.ForEach(x => sb.Append(" \"" + x + "\""));
			var lines = new List<string>();
			run(sb.ToString(), (m) => lines.Add(m));
			return lines.ToArray();
		}

		public void Run(string[] arguments)
		{
			foreach (var line in Query(arguments))
				_dispatch(line);
		}
		
		private IEnumerable<BaseCommandHandlerParameter> getUsages()
		{
			var commands = new List<BaseCommandHandlerParameter>();
			var usage = getUsage();
			new UsageParser(usage)
				.Parse().ToList()
					.ForEach(y =>
						{
							var name = y.Name;
							if (!y.Required)
								name = "[" + name + "]";
							if (y.Override)
								name = "[[" + name + "]]";
							var cmd = new BaseCommandHandlerParameter(
								name,
								y.Description,
								CommandType.FileCommand);
							y.Parameters.ToList()
								.ForEach(p => cmd.Add(p));
							commands.Add(cmd);
						});
			return commands;
		}
		
		private string getUsage()
		{
			return ToSingleLine("get-command-definitions");
		}

		private string ToSingleLine(string arguments)
		{
			return ToSingleLine(arguments, null);
		}

		private string ToSingleLine(string arguments, Process proc)
		{
			var sb = new StringBuilder();
			if (proc == null)
				run(arguments, (x) => sb.Append(x));
			else
				run(proc, arguments, (x) => sb.Append(x));
			return sb.ToString();
		}

		private void run(string arguments)
		{
			if (queryEngine(arguments, null))
				return;
			var proc = new Process();
			execute(proc, _path, arguments, null);
		}

        private void run(string arguments, Action<string> onLineReceived)
		{
			if (queryEngine(arguments, onLineReceived))
				return;
			var proc = new Process();
			execute(
				proc,
				_path,
				arguments,
				(error, x) => {
						if (error) {
							Logger.Write(
								string.Format("Failed running {0} with {1}", _path, arguments));
							Logger.Write(x);
							return;
						}
						onLineReceived(x);
					});
		}

		private void run(Process proc, string arguments, Action<string> onLineReceived)
		{
			Logger.Write("Running {0} {1}", _path, arguments);
			execute(
				proc,
				_path,
				arguments,
				(error, x) => {
						if (error) {
							Logger.Write(
								string.Format("Failed running {0} with {1}", _path, arguments));
							Logger.Write(x);
							return;
						}
						onLineReceived(x);
					});
			Logger.Write("Done - Running {0} {1}", _path, arguments);
		}

		private void execute(Process proc, string cmd, string arguments, Action<bool, string> onLineReceived)
		{
			Logger.Write("Executing {0} {1}", cmd, arguments);
            if (onLineReceived != null)
                proc.Query(cmd, arguments, false, Environment.CurrentDirectory, onLineReceived);
			else
            	proc.Run(cmd, arguments, false, Environment.CurrentDirectory);
		}

		private bool queryEngine(string arguments, Action<string> onLineReceived) {
			Logger.Write("About to query {0} {1}", _path, arguments);
			if (_process != null && !_process.HasExited) {
				// TODO: Instad of locking use correlationID to be able to run requests in parallel
				Logger.Write("Querying {0} {1}", _path, arguments);
				lock (_processLock) {
					_isQuerying = true;
					if (onLineReceived != null)
						_responseDispatcher = onLineReceived;
					_process.StandardInput.WriteLine(arguments);
					while (_isQuerying)
						Thread.Sleep(10);
					if (onLineReceived != null)
						_responseDispatcher = (m) => {};
				}
				Logger.Write("Done - Querying {0} {1}", _path, arguments);
				return true;
			}
			Logger.Write("Process is not ready {0} {1}", _path, arguments);
			if (_process == null) {
				Logger.Write("Process has not been started");
			} else if (_process.HasExited) {
				Logger.Write("Process has exited");
			}
			return false;
		}
	}

	public class SignatureLocation
	{
		public string File { get; private set; }
		public string Signature { get; private set; }
		public Position Start { get; private set; }
		public Position End { get; private set; }

		public SignatureLocation(string file, string signature, Position start, Position end)
		{
			File = file;
			Signature = signature;
			Start = start;
			End = end;
		}
	}
}
