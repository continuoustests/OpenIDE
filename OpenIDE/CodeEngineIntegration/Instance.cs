using System;
using System.IO;
using System.Text;
namespace OpenIDE.CodeEngineIntegration
{
	public class Instance
	{
		private Func<OpenIDE.EditorEngineIntegration.IClient> _clientFactory;
		
		public string File { get; private set; }
		public int ProcessID { get; private set; }
		public string Key { get; private set; }
		public int Port { get; private set; }
		
		public Instance(Func<OpenIDE.EditorEngineIntegration.IClient> clientFactory, string file, int processID, string key, int port)
		{
			_clientFactory = clientFactory;
			File = file;
			ProcessID = processID;
			Key = key;
			Port = port;
		}
		
		public static Instance Get(Func<OpenIDE.EditorEngineIntegration.IClient> clientFactory, string file, string[] lines)
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
			send("gototype");
		}

		public void Explore()
		{
			send("explore");
		}
		
		public string GetProjects(string query)
		{
			return queryCodeEngine("get-projects", query);
		}

		public string GetFiles(string query)
		{
			return queryCodeEngine("get-files", query);
		}

		public string GetCodeRefs(string query)
		{
			return queryCodeEngine("get-signatures", query);
		}

		public string GetSignatureRefs(string query)
		{
			return queryCodeEngine("get-signature-refs", query);
		}

		public string FindTypes(string query)
		{
			return queryCodeEngine("find-types", query);
		}

		public void SnippetComplete(string[] arguments)
		{
			sendArgumentCommand("snippet-complete", arguments);
		}

		public void SnippetCreate(string[] arguments)
		{
			sendArgumentCommand("snippet-create", arguments);
		}
		
		public void MemberLookup(string[] arguments)
		{
			sendArgumentCommand("member-lookup", arguments);
		}

		public void GoToDefinition(string[] arguments)
		{
			sendArgumentCommand("goto-defiinition", arguments);
		}

		private void sendArgumentCommand(string command, string[] arguments)
		{
			var sb = new StringBuilder();
			sb.Append(command);
			foreach (var arg in arguments)
				sb.Append(" \"" + arg + "\"");
			send(sb.ToString());
		}

		private string queryCodeEngine(string command, string query)
		{
			var client = _clientFactory.Invoke();
			client.Connect(Port, (s) => {});
			if (!client.IsConnected)
				return "";
			var reply = client.Request(command + " " + query);
			client.Disconnect();
			return reply;
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

