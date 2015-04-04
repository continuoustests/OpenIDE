using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using OpenIDE.CodeEngine.Core.Endpoints.Tcp;
using OpenIDE.Core.FileSystem;
using OpenIDE.Core.Logging;

namespace OpenIDE.CodeEngine.Core.EditorEngine
{
	public class Editor : IDisposable
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

		public void Dispose()
		{
			if (_client != null && _client.IsConnected)
				_client.Disconnect();
			_client = null;
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
			return instances.Where(x => isSameInstance(x.Key, path) && canConnectTo(x))
				.OrderByDescending(x => x.Key.Length)
				.FirstOrDefault();
		}

		private bool isSameInstance(string running, string suggested)
		{
			return running == suggested || suggested.StartsWith(running+Path.DirectorySeparatorChar.ToString());
		}
		
		private IEnumerable<Instance> getInstances(string path)
		{
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace(Path.DirectorySeparatorChar.ToString(), "-");
            var filename = string.Format("*.EditorEngine.{0}.pid", user);
			var dir = FS.GetTempPath();
			if (Directory.Exists(dir))
			{
				foreach (var file in Directory.GetFiles(dir, filename))
				{
					Instance instance;
					try {
						instance = Instance.Get(file, File.ReadAllLines(file));
					} catch {
						instance = null;
					}
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
			if (!connected) {
				try {
					Process.GetProcessById(info.ProcessID);
				} catch {
					File.Delete(info.File);
				}
			}
			return connected;
		}
	}
}

