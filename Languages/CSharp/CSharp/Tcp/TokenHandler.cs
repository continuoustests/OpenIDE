using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharp.Tcp
{
    public class TokenHandler
    {
        public static string WriteInstanceInfo(string key, int port)
		{
			var path = Path.Combine(Path.GetTempPath(), "CSharp.Plugin");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			var instanceFile = Path.Combine(path, string.Format("{0}.pid", Process.GetCurrentProcess().Id));
			var sb = new StringBuilder();
			sb.AppendLine(key);
			sb.AppendLine(port.ToString());
			File.WriteAllText(instanceFile, sb.ToString());
            return instanceFile;
		}

        public static CSharpClient GetClient(string key, Action<string> onMessage)
        {
            return new CSharpClient(key, onMessage);
        }
    }

    public class CSharpClient
    {
        private string _path = null;
		private SocketClient _client = null;
		
		public Action<string> _onMessage;
		
		public bool IsConnected { get { return isConnected(); } }
		
		internal  CSharpClient(string path, Action<string> onMessage) {
			_path = path;
            _onMessage = onMessage;
			if (_client != null &&_client.IsConnected)
				_client.Disconnect();
			_client = null;
			isConnected();
		}

		public void Request(string message) {
			if (!isConnected())
				return;
			_client.Request(message);
		}
		
		private bool isConnected() {
			try
			{
				if (_client != null && _client.IsConnected)
					return true;
				var instance = new CSharpPluginLocator().GetInstance(_path);
				if (instance == null)
					return false;
				_client = new SocketClient();
				_client.Connect(instance.Port, (m) => {
						if (_onMessage != null)
							_onMessage(m);
					});
				if (_client.IsConnected)
					return true;
				_client = null;
				return false;
			}
			catch
			{
				return false;
			}
		}
	}
	
	class CSharpPluginLocator
	{
		public Token GetInstance(string path) {
			var instances = getInstances(path);
			return instances.Where(x => path.StartsWith(x.Key) && CanConnectTo(x))
				.OrderByDescending(x => x.Key.Length)
				.FirstOrDefault();
		}
		
		private IEnumerable<Token> getInstances(string path) {
			var dir = Path.Combine(Path.GetTempPath(), "CSharp.Plugin");
			if (Directory.Exists(dir))
			{
				foreach (var file in Directory.GetFiles(dir, "*.pid"))
				{
					var instance = Token.Get(file, File.ReadAllLines(file));
					if (instance != null)
						yield return instance;
				}
			}
		}
		
		public bool CanConnectTo(Token info)
		{
			var client = new SocketClient();
			client.Connect(info.Port, (s) => {});
			var connected = client.IsConnected;
			client.Disconnect();
			if (!connected)
				File.Delete(info.File);
			return connected;
		}
    }

    class Token
	{
		public string File { get; private set; }
		public int ProcessID { get; private set; }
		public string Key { get; private set; }
		public int Port { get; private set; }
		
		public Token(string file, int processID, string key, int port)
		{
			File = file;
			ProcessID = processID;
			Key = key;
			Port = port;
		}
		
		public static Token Get(string file, string[] lines)
		{
			if (lines.Length != 2)
				return null;
			int processID;
			if (!int.TryParse(Path.GetFileNameWithoutExtension(file), out processID))
				return null;
			int port;
			if (!int.TryParse(lines[1], out port))
				return null;
			return new Token(file, processID, lines[0], port);
		}
	}
}
