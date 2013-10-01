using System;
using System.IO;
using System.Text;
namespace OpenIDE.Core.CodeEngineIntegration
{
    public interface ICodeEngineInstanceSimple : IDisposable
    {
        string File { get; }
        int ProcessID { get; }
        string Key { get; }
        int Port { get; }
        void KeepAlive();
        void Send(string message);
        string Query(string query); 
    }

    public interface ICodeEngineInstance : IDisposable
    {
        string File { get; }
        int ProcessID { get; }
        string Key { get; }
        int Port { get; }
        void KeepAlive();
        void GoToType();
        void Explore();
        string GetProjects(string query);
        string GetFiles(string query);
        string GetCodeRefs(string query);
        string GetSignatureRefs(string query);
        string FindTypes(string query);
        void SnippetComplete(string[] arguments);
        void SnippetCreate(string[] arguments);
        void SnippetEdit(string[] arguments);
        void SnippetDelete(string[] arguments);
        void MemberLookup(string[] arguments);
        void GoToDefinition(string[] arguments);
        string GetRScriptState(string scriptName); 
        void Shutdown();
    }

    public class Instance : ICodeEngineInstance, ICodeEngineInstanceSimple
    {
        private bool _keepClientAlive = false;
        private EditorEngineIntegration.IClient _client;
		private Func<EditorEngineIntegration.IClient> _clientFactory;
		
		public string File { get; private set; }
		public int ProcessID { get; private set; }
		public string Key { get; private set; }
		public int Port { get; private set; }
		
		public Instance(Func<EditorEngineIntegration.IClient> clientFactory, string file, int processID, string key, int port)
		{
			_clientFactory = clientFactory;
			File = file;
			ProcessID = processID;
			Key = key;
			Port = port;
		}

        public void KeepAlive()
        {
            _keepClientAlive = true;
        }
		
		public static Instance Get(Func<EditorEngineIntegration.IClient> clientFactory, string file, string[] lines)
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
			Send("gototype");
		}

		public void Explore()
		{
			Send("explore");
		}
		
		public string GetProjects(string query)
		{
			return Query("get-projects " + query);
		}

		public string GetFiles(string query)
		{
			return Query("get-files " + query);
		}

		public string GetCodeRefs(string query)
		{
			return Query("get-signatures " + query);
		}

		public string GetSignatureRefs(string query)
		{
			return Query("get-signature-refs " + query);
		}

		public string FindTypes(string query)
		{
			return Query("find-types " + query);
		}

		public void SnippetComplete(string[] arguments)
		{
			sendArgumentCommand("snippet-complete", arguments);
		}

		public void SnippetCreate(string[] arguments)
		{
			sendArgumentCommand("snippet-create", arguments);
		}

		public void SnippetEdit(string[] arguments)
		{
			sendArgumentCommand("snippet-edit", arguments);
		}
		
		public void SnippetDelete(string[] arguments)
		{
			sendArgumentCommand("snippet-delete", arguments);
		}

		public void MemberLookup(string[] arguments)
		{
			sendArgumentCommand("member-lookup", arguments);
		}

		public void GoToDefinition(string[] arguments)
		{
			sendArgumentCommand("goto-defiinition", arguments);
		}

		public string GetRScriptState(string scriptName)
		{
			return Query("rscript-state " + scriptName);
		}

		public void Shutdown()
		{
			Send("shutdown");
		}

		private void sendArgumentCommand(string command, string[] arguments)
		{
			var sb = new StringBuilder();
			sb.Append(command);
			foreach (var arg in arguments)
				sb.Append(" \"" + arg + "\"");
			Send(sb.ToString());
		}

		public string Query(string command)
		{
            if (_client == null) {
			    _client = _clientFactory.Invoke();
			    _client.Connect(Port, (s) => {});
			    if (!_client.IsConnected)
				    return "";
            }
			var reply = _client.Request(command);
			if (!_keepClientAlive) {
			    _client.Disconnect();
                _client = null;
            }
			return reply;
		}
		
		public void Send(string message)
		{
            if (_client == null) {
			    _client = _clientFactory.Invoke();
			    _client.Connect(Port, (s) => {});
			    if (!_client.IsConnected)
				    return;
            }
			_client.SendAndWait(message);
            if (!_keepClientAlive) {
			    _client.Disconnect();
                _client = null;
            }
		}

        public void Dispose()
        {
            if (_client != null)
                _client.Disconnect();
        }
    }
}

