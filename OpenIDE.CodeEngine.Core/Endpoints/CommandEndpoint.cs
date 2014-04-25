using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.CommandBuilding;
using OpenIDE.CodeEngine.Core.UI;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.EditorEngine;
using OpenIDE.CodeEngine.Core.Endpoints.Tcp;
using OpenIDE.Core.Commands;
using OpenIDE.Core.Logging;
namespace OpenIDE.CodeEngine.Core.Endpoints
{
	public class CommandEndpoint
	{
		private string _keyPath;
		private TcpServer _server;
		private Editor _editor;
		private ITypeCache _cache;
		private EventEndpoint _eventEndpoint;
		private string _instanceFile;
		private FileStream _instanceLock;
		private List<Action<MessageArgs,ITypeCache,Editor>> _handlers =
			new List<Action<MessageArgs,ITypeCache,Editor>>();
		
		public bool IsAlive { get { return _editor.IsConnected; } }
		public Editor Editor { get { return _editor; } }
		public string Token { get { return _keyPath; } }
		
		public CommandEndpoint(string editorKey, ITypeCache cache, EventEndpoint eventEndpoint)
		{
			Logger.Write("Initializing command endpoint using editor key " + editorKey);
			_keyPath = editorKey;
			_cache = cache;
			Logger.Write("Setting up event endpoint");
			_eventEndpoint = eventEndpoint;
			_eventEndpoint.DispatchThrough((m) => {
					handle(new MessageArgs(Guid.Empty, m));
				});
			_server = new TcpServer();
			_server.IncomingMessage += Handle_serverIncomingMessage;
			_server.Start();
			Logger.Write("CodeEngine started listening on port {0}", _server.Port);
			_editor = new Editor();
			Logger.Write("Binding editor RecievedMessage");
			_editor.RecievedMessage += Handle_editorRecievedMessage;
			Logger.Write("Connecting to editor");
			_editor.Connect(_keyPath);
			Logger.Write("Done - Connecting to editor");
		}

		void Handle_editorRecievedMessage(object sender, MessageArgs e)
		{
			handle(e.Message);
		}
		 
		void Handle_serverIncomingMessage (object sender, MessageArgs e)
		{
			handle(e);
		}

		void handle(string commandMessage)
		{
			var msg = CommandMessage.New(commandMessage);
			var command = new CommandStringParser().GetArgumentString(msg.Arguments.ToArray());
			var fullCommand = msg.Command + " " + command;
			handle(new MessageArgs(Guid.Empty, fullCommand.Trim()));
		}

		void handle(MessageArgs command)
		{
			Logger.Write("Handling incoming message: " + command.Message);
			_eventEndpoint.Send(command.Message);
			ThreadPool.QueueUserWorkItem((cmd) =>
				{
					_handlers
						.ForEach(x => x(command, _cache, _editor));
				}, null);
		}

		public void AddHandler(Action<MessageArgs,ITypeCache,Editor> handler)
		{
			_handlers.Add(handler);
		}

		public void Handle(string command)
		{
			handle(command);
		}

		public void PublishEvent(string body)
		{
			_eventEndpoint.Send(body);
		}
		
		public void Send(string message)
		{
			_server.Send(message);
		}

		public void Send(string message, Guid clientID)
		{
			_server.Send(message, clientID);
		}
		
		public void Start()
		{
			_server.Start();
			writeInstanceInfo(_keyPath);
			_eventEndpoint.Send("codeengine started");
		}
		
		public void Stop()
		{
			_eventEndpoint.Send("codeengine stopped");
			if (File.Exists(_instanceFile)) {
				_instanceLock.Close();
				File.Delete(_instanceFile);
			}
		}
		
		private void writeInstanceInfo(string key)
		{
			var path = Path.Combine(Path.GetTempPath(), "OpenIDE.CodeEngine");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			_instanceFile = Path.Combine(path, string.Format("{0}.pid", Process.GetCurrentProcess().Id));
			var sb = new StringBuilder();
			sb.AppendLine(key);
			sb.AppendLine(_server.Port.ToString());
			File.WriteAllText(_instanceFile, sb.ToString());
			_instanceLock = new FileStream(_instanceFile, FileMode.Open, FileAccess.Read);
		}
	}
}

