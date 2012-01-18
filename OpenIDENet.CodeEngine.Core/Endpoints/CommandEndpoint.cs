using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDENet.CodeEngine.Core.UI;
using OpenIDENet.CodeEngine.Core.Caching;
using OpenIDENet.CodeEngine.Core.Commands;
using OpenIDENet.CodeEngine.Core.EditorEngine;
using OpenIDENet.CodeEngine.Core.Endpoints.Tcp;
namespace OpenIDENet.CodeEngine.Core.Endpoints
{
	public class CommandEndpoint
	{
		private string _keyPath;
		private TcpServer _server;
		private Editor _editor;
		private ITypeCache _cache;
		private string _instanceFile;
		private List<Action<string,ITypeCache,Editor>> _handlers = new List<Action<string,ITypeCache,Editor>>();
		
		public bool IsAlive { get { return _editor.IsConnected; } }
		
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
			var msg = CommandMessage.New(e.Message);
			if (msg.Command == "keypress" && msg.Arguments.Count == 1 && msg.Arguments[0] == "t")
				handle("gototype");
            if (msg.Command == "keypress" && msg.Arguments.Count == 1 && msg.Arguments[0] == "e")
                handle("explore");
			else if (msg.Command == "keypress" && msg.Arguments.Count == 1 && msg.Arguments[0] == "nobuffers")
				handle("gototype");
		}
		 
		void Handle_serverIncomingMessage (object sender, MessageArgs e)
		{
			if (e.Message == "GoToType")
				handle("gototype");
			else if (e.Message == "Explore")
				handle("explore");
			else
				handle(e.Message);

		}

		void handle(string command)
		{
			ThreadPool.QueueUserWorkItem((cmd) =>
				{
					_handlers
						.ForEach(x => x(command.ToString(), _cache, _editor));
				}, command);
		}

		public void AddHandler(Action<string,ITypeCache,Editor> handler)
		{
			_handlers.Add(handler);
		}
		
		public void Send(string message)
		{
			_server.Send(message);
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
			var path = Path.Combine(Path.GetTempPath(), "OpenIDENet.CodeEngine");
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

