using System;
using System.IO;
using System.Text;
using System.Linq;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.EditorEngineIntegration
{
	public class Instance
	{
		private Func<IClient> _clientFactory;
		
		public string File { get; private set; }
		public int ProcessID { get; private set; }
		public string Key { get; private set; }
		public int Port { get; private set; }
		
		public bool IsInitialized { get { return getEditor() != ""; } }

		public Instance(Func<IClient> clientFactory, string file, int processID, string key, int port)
		{
			_clientFactory = clientFactory;
			File = file;
			ProcessID = processID;
			Key = key;
			Port = port;
		}
		
		public static Instance Get(Func<IClient> clientFactory, string file, string[] lines)
		{
			if (lines.Length != 2)
				return null;
			int processID;
			if (!int.TryParse(Path.GetFileNameWithoutExtension(file), out processID))
				return null;
			int port;
			if (!int.TryParse(lines[1], out port))
				return null;
			return new Instance(clientFactory, file, processID, lines[0], port);
		}
		
		public string Start(string[] arguments)
		{
			var client = _clientFactory.Invoke();
			client.Connect(Port, (s) => {});
			if (!client.IsConnected)
				return "";
			var query = "editor";
			foreach (var arg in arguments)
				query += " \"" + arg + "\"";
			var reply = client.Request(query);
			Logger.Write("Editor engine started on port " + Port.ToString() + " responding with " + reply);
			client.Disconnect();
			return reply;
		}

		public void Send(string command)
		{
			send(command);
		}
		
		public void GoTo(string file, int line, int column)
		{
			send(string.Format("goto {0}|{1}|{2}", file, line, column));
		}
		
		public void SetFocus()
		{
			send("setfocus");
		}

		public string GetDirtyFiles(string file)
		{
			var client = _clientFactory.Invoke();
			client.Connect(Port, (s) => {});
			if (!client.IsConnected) {
				Logger.Write("Editor is not connected.");
				return "";
			}
			var query = "get-dirty-files";
			if (file != null)
				query += " " + file;
			var reply = client.Request(query);
			client.Disconnect();
			return reply;
		}

		public string GetCaret()
		{
			var client = _clientFactory.Invoke();
			client.Connect(Port, (s) => {});
			if (!client.IsConnected) {
				Logger.Write("Editor is not connected.");
				return "";
			}
			var query = "get-caret";
			var reply = client.Request(query);
			client.Disconnect();
			return reply;
		}

		public void Run(string[] arguments)
		{
			var sb = new StringBuilder();
			foreach (var argument in arguments)
				sb.Append("\"" + argument + "\" ");
			Logger.Write("Sending to editor: " + sb.ToString().Trim());
			send(sb.ToString().Trim());
		}

		public void UserSelect(string id, string itemlist)
		{
			send("user-select \"" + id + "\" \"" + itemlist + "\"");
		}

		private string getEditor()
		{
			var client = _clientFactory.Invoke();
			client.Connect(Port, (s) => {});
			if (!client.IsConnected)
				return "";
			var reply = client.Request("is-initialized");
			client.Disconnect();
			Console.WriteLine("initialized reply: " + reply);
			return reply;
		}
		
		private void send(string message)
		{
			var client = _clientFactory.Invoke();
			Logger.Write("Connecting to port " + Port.ToString());
			client.Connect(Port, (s) => {});
			if (!client.IsConnected) {
				Logger.Write("Editor is not connected. Not sending " + message);
				return;
			}
			client.SendAndWait(message);
			client.Disconnect();
		}
	}
}

