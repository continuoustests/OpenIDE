using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.FileSystem;

namespace OpenIDE.Core.CodeEngineIntegration
{
	public interface ICodeEngineLocator
	{
		List<Instance> GetInstances();
		Instance GetInstance(string path);
	}
	
	public class CodeEngineDispatcher : ICodeEngineLocator
	{
		private IFS _fs;
		
		public Func<EditorEngineIntegration.IClient> ClientFactory { private get; set; }
		
		public CodeEngineDispatcher(IFS fs)
		{
			_fs = fs;
			ClientFactory = () => { return new EditorEngineIntegration.Client(); };
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
            var filename = string.Format("*.OpenIDE.CodeEngine.{0}.pid", user);
			var dir = FS.GetTempPath();
			if (_fs.DirectoryExists(dir))
			{
				foreach (var file in _fs.GetFiles(dir, filename, SearchOption.TopDirectoryOnly))
				{
					var instance = Instance.Get(ClientFactory, file, _fs.ReadLines(file));
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

