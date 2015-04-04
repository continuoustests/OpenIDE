using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenIDE.CodeEngine.Core.Endpoints.Tcp;
using OpenIDE.CodeEngine.Core.ReactiveScripts;
using OpenIDE.Core.CommandBuilding;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Language;
using OpenIDE.Core.Logging;

namespace OpenIDE.CodeEngine.Core.Endpoints
{
	public class EventEndpoint
	{
		private string _keyPath;
		private TcpServer _server;
		private string _instanceFile;
		private ReactiveScriptEngine _reactiveEngine;
		private Action<string> _dispatch = (m) => {};
		private CommandStringParser _commandParser = new CommandStringParser();
		private OpenIDE.CodeEngine.Core.Endpoints.OutputEndpoint _outputEndpoint;

		public int Port { get { return _server.Port; } }
		
		public EventEndpoint(string keyPath, PluginLocator locator, OpenIDE.CodeEngine.Core.Endpoints.OutputEndpoint outputEndpoint)
		{
			_keyPath = keyPath;
			_outputEndpoint = outputEndpoint;
			_server = new TcpServer();
			_server.IncomingMessage += Handle_serverIncomingMessage;
			_server.Start();
			_reactiveEngine = new ReactiveScriptEngine(_keyPath, locator, (publisher, msg) => _outputEndpoint.Send(publisher, msg), dispatch);
		}

		public void DispatchThrough(Action<string> dispatch) {
			_dispatch = dispatch;
		}

		public void WriteOutput(string publisher, string message) {
			_outputEndpoint.Send(publisher, message);
		}

		private void dispatch(string message) {
			message = _commandParser.GetArgumentString(_commandParser.Parse(message).ToArray(), "'");
			Logger.Write("Event dispatching: " + message);
			_dispatch(message);
		}
 
		void Handle_serverIncomingMessage (object sender, MessageArgs e)
		{
			handle(e);
		}

		void handle(MessageArgs command)
		{
			var message = _commandParser.GetArgumentString(_commandParser.Parse(command.Message).ToArray(), "'");
			Logger.Write("Event handle: " + message);
            _reactiveEngine.Handle(message);
		}
		
		public void Send(string message)
		{
			message = _commandParser.GetArgumentString(_commandParser.Parse(message).ToArray(), "'");
			Logger.Write("Event send: " + message);
			_server.Send(message);
			_reactiveEngine.Handle(message);
		}
		
		public void Start()
		{
			_server.Start();
			writeInstanceInfo(_keyPath);
		}
		
		public void Stop()
		{
			if (File.Exists(_instanceFile))
				File.Delete(_instanceFile);
			_reactiveEngine.Shutdown();
		}

		public string GetScriptState(string name)
		{
			return _reactiveEngine.GetState(name);
		}
		
		private void writeInstanceInfo(string key)
		{
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace(Path.DirectorySeparatorChar.ToString(), "-");
            var filename = string.Format("{0}.OpenIDE.Events.{1}.pid", Process.GetCurrentProcess().Id, user);
            _instanceFile = Path.Combine(FS.GetTempPath(), filename);
			var sb = new StringBuilder();
			sb.AppendLine(key);
			sb.AppendLine(_server.Port.ToString());
			File.WriteAllText(_instanceFile, sb.ToString());
		}
	}
}

