using System;
using System.IO;
namespace OpenIDENet.CodeEngineIntegration
{
	public class Instance
	{
		private Func<OpenIDENet.EditorEngineIntegration.IClient> _clientFactory;
		
		public string File { get; private set; }
		public int ProcessID { get; private set; }
		public string Key { get; private set; }
		public int Port { get; private set; }
		
		public Instance(Func<OpenIDENet.EditorEngineIntegration.IClient> clientFactory, string file, int processID, string key, int port)
		{
			_clientFactory = clientFactory;
			File = file;
			ProcessID = processID;
			Key = key;
			Port = port;
		}
		
		public static Instance Get(Func<OpenIDENet.EditorEngineIntegration.IClient> clientFactory, string file, string[] lines)
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
		
		public void GoToType()
		{
			send("GoToType");
		}

		public void Explore()
		{
			send("Explore");
		}
		
		public void GetProjects(string query)
		{
			queryCodeEngine("get-projects", query);
		}

		public void GetFiles(string query)
		{
			queryCodeEngine("get-files", query);
		}

		public void GetCodeRefs(string query)
		{
			queryCodeEngine("get-signatures", query);
		}

		public void GetSignatureRefs(string query)
		{
			queryCodeEngine("get-signature-refs", query);
		}

		private void queryCodeEngine(string command, string query)
		{
			var client = _clientFactory.Invoke();
			client.Connect(Port, (s) => {});
			if (!client.IsConnected)
				return;
			var reply = client.Request(command + " " + query);
			Console.WriteLine(reply);
			client.Disconnect();
		}
		
		private void send(string message)
		{
			var client = _clientFactory.Invoke();
			client.Connect(Port, (s) => {});
			if (!client.IsConnected)
				return;
			client.SendAndWait(message);
			client.Disconnect();
		}
	}
}

