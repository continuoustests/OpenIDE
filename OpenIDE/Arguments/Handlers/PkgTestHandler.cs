using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Language;
using CoreExtensions;
using OpenIDE.Core.Profiles;

namespace OpenIDE.Arguments.Handlers
{
	class PkgTestHandler : ICommandHandler
	{
		private string _token;
		private string _testRunLocation;
		private string _currentTest;
		private bool _verbose;
		private bool _showoutputs;
		private bool _showevents;
		private bool _printOnlyErrorsAndInconclusives;
		private StringBuilder _summary = new StringBuilder();
		private List<string> _events;
		private List<string> _outputs;
		private List<string> _asserts;
		
		public CommandHandlerParameter Usage {
			get {
				var usage = new CommandHandlerParameter(
					"All",
					CommandType.Run,
					Command,
					"Runs tests for all or parts of the system");
				usage.Add("-v", "Verbose mode prints running tests");
				usage.Add("-o", "Prints outputs for failing tests");
				usage.Add("-e", "Prints events for failing tests");
				usage.Add("--only-errors", "Prints only failuers and inconclusives");
				return usage;
			}
		}

		public string Command { get { return "pkgtest"; } }

		public PkgTestHandler(string token) {
			_token = token;
		}

		public void Execute(string[] arguments) {
			_verbose = arguments.Contains("-v");
			_showoutputs = arguments.Contains("-o");
			_showevents = arguments.Contains("-e");
			_printOnlyErrorsAndInconclusives = arguments.Contains("--only-errors");
			var profiles = new ProfileLocator(_token);

			var testFiles = new List<string>();
			if (arguments.Length > 0 && File.Exists(arguments[0])) {
				testFiles.Add(arguments[0]);
			} else {
				testFiles
					.AddRange(
						getTests(profiles.GetLocalProfilePath("default")));
				testFiles
					.AddRange(
						getTests(profiles.GetGlobalProfilePath("default")));
				testFiles
					.AddRange(
						getTests(
							Path.GetDirectoryName(
								Assembly.GetExecutingAssembly().Location)));
			}

			foreach (var testFile in testFiles) {
				
				var systemStarted = false;
				var runCompleted = false;
				var eventListener = new Thread(() => {
							new Process()
								.Query(
									"oi",
									"event-listener",
									false,
									_testRunLocation,
									(error, line) => {
											if (line == "codeengine started")
												systemStarted = true;
											if (line == "codeengine stopped")
												runCompleted = true;
											_events.Add(line);
										});
						});
				var isQuerying = false;
				var useEditor = false;
				var tests = new List<string>();
				Process proc = null;
				try {
					_testRunLocation = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
					Directory.CreateDirectory(_testRunLocation);
					_events = new List<string>();
					_outputs = new List<string>();
					_asserts = new List<string>();

					runCommand("init");
					eventListener.Start();

					new Thread(() => {
							var testProc = new Process();
							testProc
								.Query(
									testFile,
									_testRunLocation,
									false,
									Environment.CurrentDirectory,
									(error, line) => {
											if (line == "initialized" || line.StartsWith("initialized|")) {
												var chunks = line.Split(new[] {'|'});
												if (chunks.Length > 1 && chunks[1] == "editor") {
													new Process().Run("oi", "editor test", false, _testRunLocation);
													while (!systemStarted)
														Thread.Sleep(10);
													useEditor = true;
												}
			        							proc = testProc;
			        							return;
			        						}
											if (line == "end-of-conversation") {
			        							isQuerying = false;
			        							return;
			        						}
											handleFeedback(proc, error, line);
										});
							isQuerying = false;
							runCompleted = true;
						}).Start();
				} catch (Exception ex) {
					Console.WriteLine(ex.ToString());
				}

				while (proc == null)
					Thread.Sleep(10);

				isQuerying = ask(proc, "get-tests");
				while (isQuerying)
					Thread.Sleep(10);
				tests.AddRange(
					_summary.ToString()
					.Replace("\t", "")
					.Split(
						new[] { Environment.NewLine },
						StringSplitOptions.RemoveEmptyEntries));

				foreach (var test in tests) {
					if (_currentTest != null)
						writeInconclusive();
					_outputs.Clear();
					_events.Clear();
					_asserts.Clear();
					_currentTest = test;
					_summary = new StringBuilder();
					if (_verbose)
						Console.Write(_currentTest + "...");
					isQuerying = ask(proc, "test|" + _currentTest);
					while (isQuerying)
						Thread.Sleep(10);
				}

				if (useEditor) {
					new Process().Run("oi", "editor command kill", false, _testRunLocation);
				}

				ask(proc, "shutdown");
				while (!runCompleted)
					Thread.Sleep(10);
				eventListener.Abort();

				if (Directory.Exists(_testRunLocation))
					Directory.Delete(_testRunLocation, true);

				if (_currentTest != null)
					writeInconclusive();
				_currentTest = null;
			}
		}

		private bool ask(Process proc, string msg) {
			try {
				proc.Write(msg);
				return true;
			} catch {
				return false;
			}
		}

		private void handleFeedback(Process proc, bool error, string line) {
			if (line == "passed" || line.StartsWith("passed|")) {
				handleTestDone("PASSED ", ConsoleColor.Green, !_printOnlyErrorsAndInconclusives);
			} else if (error || line == "failed" || line.StartsWith("failed|")) {
				if (error)
					_summary.AppendLine(line);
				else {
					var chunks = line.Split(new[] { '|' });
					if (chunks.Length > 1)
						_asserts.Add(chunks[1]);
				}
				handleTestDone("FAILED ", ConsoleColor.Red);

				if (_asserts.Count > 0) {
					Console.ForegroundColor = ConsoleColor.Red;
					foreach (var assert in _asserts)
						Console.WriteLine("\t" + assert);
					Console.ResetColor();
				}

				if (_showoutputs && _outputs.Count > 0) {
					Console.WriteLine("\tOutputs:");
					foreach (var output in _outputs)
						Console.WriteLine("\t" + output);
					Console.WriteLine();
				}
				
				if (_showevents && _events.Count > 0) {
					Console.WriteLine("\tEvents:");
					foreach (var @event in _events)
						Console.WriteLine("\t" + @event);
					Console.WriteLine();
				}
			} else if (line.StartsWith("command|")) {
				runCommand(line.Substring(8, line.Length - 8));
			} else if (line.StartsWith("hasoutput|")) {
				var pattern = line.Substring(10, line.Length - 10);
				var result = _outputs.Any(x => x.Trim() == pattern.Trim());
				if (!result)
					_asserts.Add("Expected (output): " + pattern);
				ask(proc, result.ToString().ToLower());
			} else if (line.StartsWith("hasevent|")) {
				var pattern = line.Substring(9, line.Length - 9);
				var result = _events.Any(x => x.Trim() == pattern.Trim());
				if (!result)
					_asserts.Add("Expected (event): " + pattern);
				ask(proc, result.ToString().ToLower());
			} else {
				_summary.AppendLine("\t" + line);
			}

		}

		private void runCommand(string command) {
			runCommand(command, true);
		}

		private void runCommand(string command, bool listenForFeedback) {
			new Process()
					.Query(
						"oi",
						command,
						false,
						_testRunLocation,
						(error, line) => {
								if (!listenForFeedback)
									return;
								if (error)
									handleFeedback(null, true, line);
								else
									_outputs.Add(line);
							});
		}

		private void handleTestDone(string state, ConsoleColor color) {
			handleTestDone(state, color, true);
		}

		private void handleTestDone(string state, ConsoleColor color, bool print) {
			if (print) {
				Console.SetCursorPosition(0, Console.CursorTop);
				Console.ForegroundColor = color;
				Console.Write(state);
				Console.ResetColor();
				Console.WriteLine(_currentTest + "        ");
				writeSummary();
			}
			_currentTest = null;
		}

		private void writeInconclusive() {
			handleTestDone("?????? ", ConsoleColor.Yellow);
		}

		private void writeSummary() {
			var summary = _summary.ToString();
			if (summary.Replace(Environment.NewLine, "").Length > 0) {
				Console.WriteLine(summary);
				//if (!summary.EndsWith(Environment.NewLine))
				//	Console.WriteLine();
			}
		}

		private IEnumerable<string> getTests(string path) {
			return Directory
				.GetFiles(path, "*.oi-pkg-tests.*", SearchOption.AllDirectories)
				.Where(x => Path.GetExtension(x) != ".swp");
		}
	}
}