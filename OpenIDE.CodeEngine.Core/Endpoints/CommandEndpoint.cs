using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.CodeEngine.Core.UI;
using OpenIDE.CodeEngine.Core.Caching;
using OpenIDE.CodeEngine.Core.Commands;
using OpenIDE.CodeEngine.Core.EditorEngine;
using OpenIDE.CodeEngine.Core.Endpoints.Tcp;
using OpenIDE.CodeEngine.Core.Logging;
namespace OpenIDE.CodeEngine.Core.Endpoints
{
	public class CommandEndpoint
	{
		private string _keyPath;
		private TcpServer _server;
		private Editor _editor;
		private ITypeCache _cache;
		private string _instanceFile;
		private List<Action<MessageArgs,ITypeCache,Editor>> _handlers =
			new List<Action<MessageArgs,ITypeCache,Editor>>();
		
		public bool IsAlive { get { return _editor.IsConnected; } }
		public Editor Editor { get { return _editor; } }
		
		public CommandEndpoint(string editorKey, ITypeCache cache)
		{
			_keyPath = editorKey;
			_cache = cache;
			_server = new TcpServer();
			_server.IncomingMessage += Handle_serverIncomingMessage;
			_server.Start();
			_editor = new Editor();
			_editor.RecievedMessage += Handle_editorRecievedMessage;
			_editor.Connect(_keyPath);
		}

		void Handle_editorRecievedMessage(object sender, MessageArgs e)
		{
			Logger.Write(e.Message);
			var msg = CommandMessage.New(e.Message);
			if (msg.Command == "keypress" && msg.Arguments.Count == 1 && msg.Arguments[0] == "t")
				handle(new MessageArgs(Guid.Empty, "gototype"));
            if (msg.Command == "keypress" && msg.Arguments.Count == 1 && msg.Arguments[0] == "e")
                handle(new MessageArgs(Guid.Empty, "explore"));
			else if (msg.Command == "keypress" && msg.Arguments.Count == 1 && msg.Arguments[0] == "nobuffers")
				handle(new MessageArgs(Guid.Empty, "gototype"));
			else if (msg.Command == "keypress" && msg.Arguments.Count > 1 && msg.Arguments[0] == "snippet-complete")
				handle(new MessageArgs(Guid.Empty, e.Message.Substring(9, e.Message.Length - 9)));
			else if (msg.Command == "keypress" && msg.Arguments.Count > 1 && msg.Arguments[0] == "snippet-create")
				handle(new MessageArgs(Guid.Empty, e.Message.Substring(9, e.Message.Length - 9)));
		}
		 
		void Handle_serverIncomingMessage (object sender, MessageArgs e)
		{
			if (e.Message == "GoToType")
				handle(new MessageArgs(e.ClientID, "gototype"));
			else if (e.Message == "Explore")
				handle(new MessageArgs(e.ClientID, "explore"));
			else
				handle(e);
		}

		void handle(MessageArgs command)
		{
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
		}
		
		public void Stop()
		{
			if (File.Exists(_instanceFile))
				File.Delete(_instanceFile);
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
		}
	}
}

