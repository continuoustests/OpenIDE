using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenIDE.Core.FileSystem;

namespace OpenIDE.Core.EditorEngineIntegration
{
	public class EngineLocator : ILocateEditorEngine
	{
		private IFS _fs;
		
		public Func<IClient> ClientFactory { private get; set; }
		
		public EngineLocator(IFS fs)
		{
			_fs = fs;
			ClientFactory = () => { return new Client(); };
		}

		public List<Instance> GetInstances()
		{
			return getInstances().Where(x => canConnectTo(x)).ToList();
		}
		
		public Instance GetInstance(string path)
		{
			var instances = getInstances();
			return instances.Where(x => isSameInstance(x.Key, path) && canConnectTo(x))
				.OrderByDescending(x => x.Key.Length)
				.FirstOrDefault();
		}

		private bool isSameInstance(string running, string suggested)
		{
			return running == suggested || suggested.StartsWith(running+Path.DirectorySeparatorChar.ToString());
		}
		
		private IEnumerable<Instance> getInstances()
		{
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace(Path.DirectorySeparatorChar.ToString(), "-");
            var filename = string.Format("*.EditorEngine.{0}.pid", user);
			var dir = FS.GetTempPath();
			if (_fs.DirectoryExists(dir))
			{
				foreach (var file in _fs.GetFiles(dir, filename, SearchOption.TopDirectoryOnly))
				{
					Instance instance;
					try {
						instance = Instance.Get(ClientFactory, file, _fs.ReadLines(file));
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
			var client = ClientFactory.Invoke();
			client.Connect(info.Port, (s) => {});
			var connected = client.IsConnected;
			client.Disconnect();
			if (!connected) {
				try {
					Process.GetProcessById(info.ProcessID);
				} catch {
					_fs.DeleteFile(info.File);
				}
			}
			return connected;
		}
	}
}

