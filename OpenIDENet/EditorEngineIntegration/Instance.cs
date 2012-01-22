using System;
using System.IO;
using System.Text;
using System.Linq;
namespace OpenIDENet.EditorEngineIntegration
{
	public class Instance
	{
		private Func<IClient> _clientFactory;
		
		public string File { get; private set; }
		public int ProcessID { get; private set; }
		public string Key { get; private set; }
		public int Port { get; private set; }
		
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
		
		public void Start(string editor)
		{
			send(string.Format("editor {0}", editor));
		}
		
		public void GoTo(string file, int line, int column)
		{
			send(string.Format("goto {0}|{1}|{2}", file, line, column));
		}
		
		public void SetFocus()
		{
			send("setfocus");
		}

		public string GetDirtyFiles()
		{
			var client = _clientFactory.Invoke();
			client.Connect(Port);
			if (!client.IsConnected)
				return "";
			client.SendAndWait("get-dirty-files");

			var then = DateTime.Now.AddSeconds(20);
			while (then > DateTime.Now)
			{
				if (client.RecievedMessage != null)
					break;
			}
			var content = client.RecievedMessage;
			client.Disconnect();
			return content;
		}

		public void Run(string[] arguments)
		{
			var sb = new StringBuilder();
			arguments.ToList()
				.ForEach(x => sb.Append(x + " "));
			send(sb.ToString());
		}
		
		private void send(string message)
		{
			var client = _clientFactory.Invoke();
			client.Connect(Port);
			if (!client.IsConnected)
				return;
			client.SendAndWait(message);
			client.Disconnect();
		}
	}
}

