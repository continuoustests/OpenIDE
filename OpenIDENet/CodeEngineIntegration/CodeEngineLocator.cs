using System;
using OpenIDENet.FileSystem;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace OpenIDENet.CodeEngineIntegration
{
	public interface ICodeEngineLocator
	{
		Instance GetInstance(string path);
	}
	
	public class CodeEngineDispatcher : ICodeEngineLocator
	{
		private IFS _fs;
		
		public Func<OpenIDENet.EditorEngineIntegration.IClient> ClientFactory { private get; set; }
		
		public CodeEngineDispatcher(IFS fs)
		{
			_fs = fs;
			ClientFactory = () => { return new OpenIDENet.EditorEngineIntegration.Client(); };
		}
		
		public Instance GetInstance(string path)
		{
			var instances = getInstances(path);
			return instances.Where(x => path.StartsWith(x.Key) && canConnectTo(x))
				.OrderByDescending(x => x.Key.Length)
				.FirstOrDefault();
		}
		
		private IEnumerable<Instance> getInstances(string path)
		{
			var dir = Path.Combine(Path.GetTempPath(), "OpenIDENet.CodeEngine");
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

