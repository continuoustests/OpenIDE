using System;
using OpenIDE.Core.Logging;
using OpenIDE.CodeEngine.Core.Endpoints.Tcp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
namespace OpenIDE.CodeEngine.Core.EditorEngine
{
	public class Editor
	{
		private string _path = null;
		private SocketClient _client = null;
		
		public event EventHandler<MessageArgs> RecievedMessage;	
	
		public bool IsConnected
		{
			get
			{
				return isConnected();
			}
		}
		
		public void Connect(string path)
		{
			Logger.Write("Trying to connect to " + path);
			_path = path;
			if (_client != null &&_client.IsConnected)
				_client.Disconnect();
			_client = null;
			isConnected();
		}
		
		public void GoTo(string file, int line, int column)
		{
			if (!isConnected())
				return;
			_client.Send(string.Format("goto {0}|{1}|{2}", file, line, column));
		}
		
		public void SetFocus()
		{
			if (!isConnected())
				return;
			_client.Send("setfocus");
		}

		public void Send(string message)
		{
			if (!isConnected())
				return;
			_client.Send(message);
		}
		
		private bool isConnected()
		{
			if (canConnect())
				return true;
			Thread.Sleep(100);
			return canConnect();
		}

		private bool canConnect() {
			try {
				if (_client != null && _client.IsConnected)
					return true;
				var instance = new EngineLocator().GetInstance(_path);
				if (instance == null)
					return false;
				Logger.Write("Connecting to port {0} hosted by {1} with token {2}", instance.Port, instance.ProcessID, instance.File);
				_client = new SocketClient();
				_client.Connect(instance.Port, (m) => {
						if (RecievedMessage != null && m != null && m.Trim().Length > 0)
							RecievedMessage(this, new MessageArgs(Guid.Empty, m));
					});
				if (_client.IsConnected) {
					Logger.Write("Connecting to editor on port: {0}", instance.Port);
					return true;
				}
				_client = null;
				return false;
			} catch (Exception ex) {
				Logger.Write(ex);
				return false;
			}
		}
	}
	
	class EngineLocator
	{
		public Instance GetInstance(string path)
		{
			var instances = getInstances(path);
			return instances.Where(x => path.StartsWith(x.Key) && canConnectTo(x))
				.OrderByDescending(x => x.Key.Length)
				.FirstOrDefault();
		}
		
		private IEnumerable<Instance> getInstances(string path)
		{
			var dir = Path.Combine(Path.GetTempPath(), "EditorEngine");
			if (Directory.Exists(dir))
			{
				foreach (var file in Directory.GetFiles(dir, "*.pid"))
				{
					var instance = Instance.Get(file, File.ReadAllLines(file));
					if (instance != null)
						yield return instance;
				}
			}
		}
		
		private bool canConnectTo(Instance info)
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
}

