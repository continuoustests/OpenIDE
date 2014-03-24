using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
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
		private bool _logging;
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
				usage.Add("[TEST-FILE]", "Specify test file to run tests for");
				usage.Add("-v", "Verbose mode prints running tests");
				usage.Add("-o", "Prints outputs for failing tests");
				usage.Add("-e", "Prints events for failing tests");
				usage.Add("-l", "Logging");
				usage.Add("--only-errors", "Prints only failuers and inconclusives");
				return usage;
			}
		}

		public string Command { get { return "packagetest"; } }

		public PkgTestHandler(string token) {
			_token = token;
		}

		private void log(string message, params object[] args) {
			if (_logging)
				Console.WriteLine(message, args);
		}

		public void Execute(string[] arguments) {
			_verbose = arguments.Contains("-v");
			_showoutputs = arguments.Contains("-o");
			_showevents = arguments.Contains("-e");
			_printOnlyErrorsAndInconclusives = arguments.Contains("--only-errors");
			_logging = arguments.Contains("-l");
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
				
				_testRunLocation = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
				Console.WriteLine("Testing: {0}", testFile);
				var eventListenerStarted = false;
				var systemStarted = false;
				var runCompleted = false;
				var eventListener = new Thread(() => {
							var eventSocketClient = new EventStuff.EventClient(
								(line) => {
											if (line == "codeengine started") {
												log("Code engine started");
												systemStarted = true;
											} if (line == "codeengine stopped") {
												log("Code engine stopped");
												runCompleted = true;
											}
											_events.Add(line);
										});
							while (true) {
								eventSocketClient.Connect(_testRunLocation);
								eventListenerStarted = true;
								if (!eventSocketClient.IsConnected) {
									Thread.Sleep(10);
									if (runCompleted || systemStarted)
										break;
									continue;
								}
								log("Event listener connected");
								while (eventSocketClient.IsConnected)
									Thread.Sleep(10);
								break;
							}
							eventListenerStarted = false;
						});
				var isQuerying = false;
				var useEditor = false;
				var tests = new List<string>();
				Process proc = null;
				try {
					Directory.CreateDirectory(_testRunLocation);
					_events = new List<string>();
					_outputs = new List<string>();
					_asserts = new List<string>();

					log("Initializing test location");
					runCommand("init");
					// Make sure we run tests in default profile is
					// this by any chance overloaded in init command
					runCommand("profile load default");
					eventListener.Start();

					new Thread(() => {
							log("Starting test process");
							var testProc = new Process();
							try {
								testProc
									.Query(
										testFile,
										_testRunLocation,
										false,
										Environment.CurrentDirectory,
										(error, line) => {
												if (line == "initialized" || line.StartsWith("initialized|")) {
													log("Test file initialized");
				        							proc = testProc;
													var chunks = line.Split(new[] {'|'});
													if (chunks.Length > 1 && chunks[1] == "editor") {
														while (!eventListenerStarted)
															Thread.Sleep(10);
														log("Starting editor");
														new Process().Run("oi", "editor test", false, _testRunLocation);
														log("Editor launched");
														useEditor = true;
													} else {
														log("System started");
														systemStarted = true;
													}
				        							return;
				        						}
												if (line == "end-of-conversation") {
				        							isQuerying = false;
				        							return;
				        						}
												handleFeedback(proc, error, line);
											});
							} catch (Exception ex) {
								handleFeedback(testProc, true, "A fatal error occured while running " + testFile + Environment.NewLine + ex.Message);
							}
							isQuerying = false;
							runCompleted = true;
						}).Start();
				} catch (Exception ex) {
					Console.WriteLine(ex.ToString());
				}

				log("Waiting for system to complete loading");
				while (!systemStarted)
					Thread.Sleep(10);

				log("Getting tests");
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
					log("Running test: " + test);
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
					log("Shuting down editor");
					new Process().Run("oi", "editor command kill", false, _testRunLocation);
				}

				log("Shuting down system");
				ask(proc, "shutdown");
				while (!runCompleted)
					Thread.Sleep(10);

				log("Waiting for event listener to stop");
				while (eventListenerStarted)
					Thread.Sleep(10);

				if (Directory.Exists(_testRunLocation))
					Directory.Delete(_testRunLocation, true);

				if (_currentTest != null)
					writeInconclusive();
				_currentTest = null;
				log("Test run finished");
				Console.WriteLine();
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
			log("Received : " + line);
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

				if (_showoutputs) {
					Console.WriteLine("\tOutputs:");
					foreach (var output in _outputs)
						Console.WriteLine("\t" + output);
					Console.WriteLine();
				}
				
				if (_showevents) {
					Console.WriteLine("\tEvents:");
					foreach (var @event in _events)
						Console.WriteLine("\t" + @event);
					Console.WriteLine();
				}
			} else if (line.StartsWith("command|")) {
				runCommand(line.Substring(8, line.Length - 8));
			} else if (line.StartsWith("hasoutput|")) {
				var pattern = line.Substring(10, line.Length - 10);
				var result = 
					retryFor5SecondsIfFalse(
						() => _outputs.Any(x => x.Trim() == pattern.Trim()));
				if (!result)
					_asserts.Add("Expected (output): " + pattern);
				ask(proc, result.ToString().ToLower());
			} else if (line.StartsWith("hasevent|")) {
				var pattern = line.Substring(9, line.Length - 9);
				var result = 
					retryFor5SecondsIfFalse(
						() => _events.Any(x => x.Trim() == pattern.Trim()));
				if (!result)
					_asserts.Add("Expected (event): " + pattern);
				ask(proc, result.ToString().ToLower());
			} else if (line == "get|applocation") {
				ask(proc, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			} else {
				_summary.AppendLine("\t" + line);
			}

		}

		private bool retryFor5SecondsIfFalse(Func<bool> check) {
			var now = DateTime.Now;
			while (true) {
				if (check())
					return true;
				if (DateTime.Now > now.AddSeconds(5))
					break;
				Thread.Sleep(50);
			}
			return false;
		}

		private void runCommand(string command) {
			runCommand(command, true);
		}

		private void runCommand(string command, bool listenForFeedback) {
			log(string.Format("Running: oi {0}, listenForFeedback={1} in {2}", command, listenForFeedback, _testRunLocation));
			var process = new Process();
			string[] errors;
			var lines = process
				.QueryAll(
					"oi",
					command,
					false,
					_testRunLocation,
					out errors);

			if (errors.Any(x => x.Trim().Length > 0)) {
				foreach (var error in errors)
					handleFeedback(null, true, error);
			}
			
			foreach (var line in lines) {
					_outputs.Add(line);
			}
			log("Process exited");
		}

		private void handleTestDone(string state, ConsoleColor color) {
			handleTestDone(state, color, true);
		}

		private void handleTestDone(string state, ConsoleColor color, bool print) {
			if (print) {
				try {
					Console.SetCursorPosition(0, Console.CursorTop);
				} catch {
				}
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
			}
		}

		private IEnumerable<string> getTests(string path) {
			if (path == null || !Directory.Exists(path))
				return new string[] {};
			return Directory
				.GetFiles(path, "*.oi-pkg-tests.*", SearchOption.AllDirectories)
				.Where(x => Path.GetExtension(x) != ".swp");
		}
	}
}

namespace EventStuff
{
	public class EventClient
	{
		private Action<string> _handler;
		private string _path = null;
		private SocketClient _client = null;
		
		public bool IsConnected { get { return isConnected(); } }
		
		public EventClient(Action<string> handler)
		{
			_handler = handler;
		}
		public void Connect(string path)
		{
			_path = path;
			if (_client != null &&_client.IsConnected)
				_client.Disconnect();
			_client = null;
			isConnected();
		}

		public void Send(string message)
		{
			if (!isConnected())
				return;
			_client.Send(message);
		}
		
		private bool isConnected()
		{
			try
			{
				if (_client != null && _client.IsConnected)
					return true;
				var instance = new EventEndpointLocator().GetInstance(_path);
				if (instance == null)
					return false;
				_client = new SocketClient();
				_client.Connect(instance.Port, (m) => _handler(m));
				if (_client.IsConnected)
					return true;
				_client = null;
				return false;
			}
			catch
			{
				return false;
			}
		}
	}
	
	class EventEndpointLocator
	{
		public Instance GetInstance(string path)
		{
			var instances = getInstances(path);
			return instances.Where(x => path.StartsWith(x.Key) && canConnectTo(x))
				.OrderByDescending(x => x.Key.Length)
				.FirstOrDefault();
		}
		
		private IEnumerable<Instance> getInstances(string path)
		{
			var dir = Path.Combine(Path.GetTempPath(), "OpenIDE.Events");
			if (Directory.Exists(dir))
			{
				foreach (var file in Directory.GetFiles(dir, "*.pid"))
				{
					var instance = Instance.Get(file, File.ReadAllLines(file));
					if (instance != null)
						yield return instance;
				}
			}
		}
		
		private bool canConnectTo(Instance info)
		{
			var client = new SocketClient();
			client.Connect(info.Port, (s) => {});
			var connected = client.IsConnected;
			client.Disconnect();
			if (!connected)
				File.Delete(info.File);
			return connected;
		}
	}

	class Instance
	{
		public string File { get; private set; }
		public int ProcessID { get; private set; }
		public string Key { get; private set; }
		public int Port { get; private set; }
		
		public Instance(string file, int processID, string key, int port)
		{
			File = file;
			ProcessID = processID;
			Key = key;
			Port = port;
		}
		
		public static Instance Get(string file, string[] lines)
		{
			if (lines.Length != 2)
				return null;
			int processID;
			if (!int.TryParse(Path.GetFileNameWithoutExtension(file), out processID))
				return null;
			int port;
			if (!int.TryParse(lines[1], out port))
				return null;
			return new Instance(file, processID, lines[0], port);
		}
	}
	
	class SocketClient
	{
		private NetworkStream _stream;
        readonly byte[] _buffer = new byte[1000000];
        private int _currentPort;
        private readonly MemoryStream _readBuffer = new MemoryStream();
        private Queue queue = new Queue();
		private bool IsSending = false;
		private Action<string> _onMessage;
		class MessageArgs : EventArgs { public string Message { get; set; } }
		private event EventHandler<MessageArgs> _messageReceived;
		
		public bool IsConnected { get; private set; }
		
		public SocketClient()
		{
			IsConnected = false;
		}

        public void Connect(int port, Action<string> onMessage)
        {
			_onMessage = onMessage;
            Connect(port, 0);
        }

        private void Connect(int port, int retryCount)
        {
            if (retryCount >= 5)
                return;
			try {
	            var client = new TcpClient();
	            client.Connect("127.0.0.1", port);
	            _currentPort = port;
	            _stream = client.GetStream();
	            _stream.BeginRead(_buffer, 0, _buffer.Length, ReadCompleted, _stream);
				IsConnected = true;
			} 
			catch 
			{
                Reconnect(retryCount);
			}
        }

        public void Disconnect()
        {
			try {
				IsConnected = false;
	            _stream.Close();
	            _stream.Dispose();
			}
			catch
			{}
        }

        private void Reconnect(int retryCount)
        {
            retryCount++;
            _readBuffer.SetLength(0);
			Disconnect();
			Connect(_currentPort, retryCount);
		}

        private void ReadCompleted(IAsyncResult result)
        {
            var stream = (NetworkStream)result.AsyncState;
            try
            {
                var x = stream.EndRead(result);
                if(x == 0) Reconnect(0);
                for (var i = 0; i < x;i++)
                {
                    if (_buffer[i] == 0)
                    {
                        var data = _readBuffer.ToArray();
                        var actual = Encoding.UTF8.GetString(data, 0, data.Length);
						if (_messageReceived != null)
							_messageReceived(this, new MessageArgs() { Message = actual });
                        _onMessage(actual);
                        _readBuffer.SetLength(0);
                    }
                    else
                    {
                        _readBuffer.WriteByte(_buffer[i]);
                    }
                }
                stream.BeginRead(_buffer, 0, _buffer.Length, ReadCompleted, stream);
            }
            catch (Exception ex)
            {
                WriteError(ex);
                Reconnect(0);
            }
        }


        public void Send(string message)
        {
            if (IsSending)
                throw new Exception("Cannot call send while doing SendAndWait, make up your mind");
            lock (queue)
            {
                queue.Enqueue(message);
                if(!IsSending) {
					SendFromQueue();                      
                }
            }
        }

        public void SendAndWait(string message)
        {
            Send(message);
            IsSending = true;
            var timeout = DateTime.Now;
            while (IsSending && DateTime.Now.Subtract(timeout).TotalMilliseconds < 8000)
                Thread.Sleep(10);
        }

		public string Request(string message)
		{
			string recieved= null;
			var correlationID = "correlationID=" + Guid.NewGuid().ToString() + "|";
			var messageToSend = correlationID + message;
			EventHandler<MessageArgs> msgHandler = (o,a) => {
					if (a.Message.StartsWith(correlationID) && a.Message != messageToSend)
						recieved = a.Message
							.Substring(
								correlationID.Length,
								a.Message.Length - correlationID.Length);
				};
			_messageReceived += msgHandler;
			Send(messageToSend);
			var timeout = DateTime.Now;
            while (DateTime.Now.Subtract(timeout).TotalMilliseconds < 8000)
			{
				if (recieved != null)
					break;
                Thread.Sleep(10);
			}
			_messageReceived -= msgHandler;
			return recieved;
		}

        private void WriteCompleted(IAsyncResult result)
        {
            var client = (NetworkStream)result.AsyncState;
            try
            {
                client.EndWrite(result);
                lock(queue)
                {
		    		IsSending = false;
                    if (queue.Count > 0)
                        SendFromQueue();
                }
                
            }
            catch (Exception ex)
            {
                WriteError(ex);
				Reconnect(0);
            }
        }

        private void SendFromQueue()
        {
            string message = null;
            lock (queue)
            {
                if (!IsSending && queue.Count > 0)
                    message = queue.Dequeue().ToString();
            }
            if (message != null)
            {
                try
                {
					byte[] toSend = Encoding.UTF8.GetBytes(message).Concat(new byte[] { 0x0 }).ToArray();
                    _stream.BeginWrite(toSend, 0, toSend.Length, WriteCompleted, _stream);
                }
                catch
                {
                }
            }
        }

        private void WriteError(Exception ex)
        {
        }
	}
}