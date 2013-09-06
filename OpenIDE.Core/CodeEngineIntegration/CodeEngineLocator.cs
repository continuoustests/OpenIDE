using System;
using System.IO;
using System.Linq;
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
			return instances.Where(x => path.StartsWith(x.Key) && canConnectTo(x))
				.OrderByDescending(x => x.Key.Length)
				.FirstOrDefault();
		}
		
		private IEnumerable<Instance> getInstances()
		{
			var dir = Path.Combine(Path.GetTempPath(), "OpenIDE.CodeEngine");
			if (_fs.DirectoryExists(dir))
			{
				foreach (var file in _fs.GetFiles(dir, "*.pid"))
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
			if (!connected)
				_fs.DeleteFile(info.File);
			return connected;
		}
	}
}

