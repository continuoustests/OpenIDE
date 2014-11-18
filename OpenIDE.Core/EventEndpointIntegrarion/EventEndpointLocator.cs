using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.FileSystem;

namespace OpenIDE.Core.EventEndpointIntegrarion
{
    public class EventClient : IDisposable
    {
        private Action<string> _handler;
        private string _path = null;
        private OpenIDE.Core.EditorEngineIntegration.Client _client = null;

        public bool IsConnected { get { return isConnected(); } }
        
        public EventClient(string path)
        {
            _path = path;
            _handler = (m) => {};
        }

        public EventClient(string path, Action<string> onMessage)
        {
            _path = path;
            _handler = onMessage;
        }

        public Instance GetInstance()
        {
            return new EventEndpointLocator().GetInstance(_path);
        }

        public void Connect()
        {
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

        public void Send(string message)
        {
            if (!isConnected())
                return;
            _client.Send(message);
        }
        
        private bool isConnected()
        {
            try
            {
                if (_client != null && _client.IsConnected)
                    return true;
                var instance = GetInstance();
                if (instance == null)
                    return false;
                _client = new OpenIDE.Core.EditorEngineIntegration.Client();
                _client.Connect(instance.Port, (m) => _handler(m));
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
    
    class EventEndpointLocator
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
            var dir = Path.Combine(FS.GetTempPath(), "OpenIDE.Events");
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
            var client = new OpenIDE.Core.EditorEngineIntegration.Client();
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