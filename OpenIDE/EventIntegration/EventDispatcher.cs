using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OpenIDE.Core.CommandBuilding;

namespace OpenIDE.EventIntegration
{
	public interface IEventDispatcher
	{
		void Forward(string command, string[] arguments);
	}

	public class EventDispatcher : IEventDispatcher
	{
		private string _path;
		
		public EventDispatcher(string path)
		{
			_path = path;
		}

		public void Forward(string command, string[] arguments)
		{
			var message = string.Format(
				"{0} {1}",
				command,
				new CommandStringParser().GetArgumentString(arguments));
			var instances = getInstances(_path);
			instances
				.Where(x => _path.StartsWith(x.Key))
				.ToList()
				.ForEach(x => trySend(x, message));
		}
		
		private IEnumerable<Instance> getInstances(string path)
		{
			var dir = Path.Combine(Path.GetTempPath(), "OpenIDE.CodeEngine");
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
		
		private bool trySend(Instance info, string message)
		{
			var client = new OpenIDE.Core.EditorEngineIntegration.Client();
			client.Connect(info.Port, (s) => {});
			var connected = client.IsConnected;
			if (connected)
				client.SendAndWait(message);
			client.Disconnect();
			if (!connected)
				File.Delete(info.File);
			return connected;
		}

		public class Instance
		{
			public string File { get; private set; }
			public int ProcessID { get; private set; }
			public string Key { get; private set; }
			public int Port { get; private set; }
		
			public Instance(string file, int processID, string key, int port)
			{
				File = file;
				ProcessID = processID;
				Key = key;
				Port = port;
			}

			public static Instance Get(string file, string[] lines)
			{
				if (lines.Length != 2)
					return null;
				int processID;
				if (!int.TryParse(Path.GetFileNameWithoutExtension(file), out processID))
					return null;
				int port;
				if (!int.TryParse(lines[1], out port))
					return null;
				return new Instance(file, processID, lines[0], port);
			}
		}
	}
}
