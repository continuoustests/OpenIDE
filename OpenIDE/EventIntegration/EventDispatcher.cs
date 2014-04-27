using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.Logging;
using OpenIDE.Core.CommandBuilding;

namespace OpenIDE.EventIntegration
{
	public interface IEventDispatcher
	{
		void Forward(string message);
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
			Forward(message);
		}

		public void Forward(string message)
		{
			Logger.Write("Forwarding event: " + message + " using token " + _path);
			var instances = getInstances(_path);
			instances
				.Where(x => _path.StartsWith(x.Key))
				.ToList()
				.ForEach(x => trySend(x, message));
		}
		
		private IEnumerable<Instance> getInstances(string path)
		{
			var dir = Path.Combine(FS.GetTempPath(), "OpenIDE.CodeEngine");
			if (Directory.Exists(dir))
			{
				foreach (var file in Directory.GetFiles(dir, "*.pid"))
				{
					var instance = Instance.Get(file, File.ReadAllLines(file));
					if (instance != null) {
						Logger.Write("Found event endpoint instance for: " + instance.Key);
						yield return instance;
					}
				}
			}
		}
		
		private bool trySend(Instance info, string message)
		{
			var client = new OpenIDE.Core.EditorEngineIntegration.Client();
			client.Connect(info.Port, (s) => {});
			var connected = client.IsConnected;
			if (connected) {
				Logger.Write("Dispatching event");
				client.SendAndWait(message);
			} else {
				Logger.Write("Could not connect to event endpoint");
			}
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

	class FS
	{
        public static string GetTempPath()
        {
            if (OS.IsOSX) {
                return "/tmp";
            }
            return Path.GetTempPath();
        }
    }

	static class OS
    {
        private static bool? _isWindows;
        private static bool? _isUnix;
        private static bool? _isOSX;

        public static bool IsWindows {
            get {
                if (_isWindows == null) {
                    _isWindows = 
                        Environment.OSVersion.Platform == PlatformID.Win32S ||
                        Environment.OSVersion.Platform == PlatformID.Win32NT ||
                        Environment.OSVersion.Platform == PlatformID.Win32Windows ||
                        Environment.OSVersion.Platform == PlatformID.WinCE ||
                        Environment.OSVersion.Platform == PlatformID.Xbox;
                }
                return (bool) _isWindows;
            }
        }

        public static bool IsPosix {
            get {
                return IsUnix || IsOSX;
            }
        }

        public static bool IsUnix {
            get {
                if (_isUnix == null)
                    setUnixAndLinux();
                return (bool) _isUnix;
            }
        }

        public static bool IsOSX {
            get {
                if (_isOSX == null)
                    setUnixAndLinux();
                return (bool) _isOSX;
            }
        }

        private static void setUnixAndLinux()
        {
            try
            {
                if (IsWindows) {
                    _isOSX = false;
                    _isUnix = false;
                } else  {
                    var process = new Process
                                      {
                                          StartInfo =
                                              new ProcessStartInfo("uname", "-a")
                                                  {
                                                      RedirectStandardOutput = true,
                                                      WindowStyle = ProcessWindowStyle.Hidden,
                                                      UseShellExecute = false,
                                                      CreateNoWindow = true
                                                  }
                                      };

                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    _isOSX = output.Contains("Darwin Kernel");
                    _isUnix = !_isOSX;
                }
            }
            catch
            {
                _isOSX = false;
                _isUnix = false;
            }
        }
    }
}
