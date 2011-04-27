using System;
using OpenIDENet.CodeEngine.Core.Endpoints.Tcp;
using System.IO;
using System.Text;
using System.Diagnostics;
using OpenIDENet.CodeEngine.Core.EditorEngine;
using OpenIDENet.CodeEngine.Core.UI;
using OpenIDENet.CodeEngine.Core.Caching;
namespace OpenIDENet.CodeEngine.Core.Endpoints
{
	public class CommandEndpoint
	{
		private TcpServer _server;
		private Editor _editor;
		private ITypeCache _cache;
		private string _instanceFile;
		
		public bool IsAlive { get { return _editor.IsConnected; } }
		
		public CommandEndpoint(string editorKey, ITypeCache cache)
		{
			_cache = cache;
			_server = new TcpServer();
			_server.IncomingMessage += Handle_serverIncomingMessage;
			_server.Start();
			_editor = new Editor();
			_editor.RecievedMessage += Handle_editorRecievedMessage;
			_editor.Connect(editorKey);
		}

		void Handle_editorRecievedMessage(object sender, MessageArgs e)
		{
			var message = EditorEngineMessage.New(e.Message);
			if (message.Command == "keypress" && message.Arguments.Count == 1 && message.Arguments[0] == "t")
				goToType();
			else if (message.Command == "keypress" && message.Arguments.Count == 1 && message.Arguments[0] == "nobuffers")
				goToType();
		}
		 
		void Handle_serverIncomingMessage (object sender, MessageArgs e)
		{
			if (e.Message == "GoToType")
				goToType();
		}
		
		private void goToType()
		{
			var form = new TypeSearchForm(_cache, (file, line, column) => { _editor.GoTo(file, line, column); }, () => { _editor.SetFocus(); });
			form.ShowDialog();
		}
		
		public void Run(string cmd)
		{
			_server.Send(cmd);
		}
		
		public void Start(string key)
		{
			_server.Start();
			writeInstanceInfo(key);
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

