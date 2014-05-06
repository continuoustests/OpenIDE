using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using OpenIDE.Core.FileSystem;

namespace OpenIDE.Core.OutputEndpointIntegration
{
    public class OutputClient
    {
        private Action<string,string> _handler;
        private string _path = null;
        private OpenIDE.Core.EditorEngineIntegration.Client _client = null;

        public bool IsConnected { get { return isConnected(); } }
        
        public OutputClient(string path)
        {
            _path = path;
            _handler = (publisher, message) => {};
        }

        public OutputClient(string path, Action<string,string> onMessage)
        {
            _path = path;
            _handler = onMessage;
        }

        public Instance GetInstance()
        {
            return new OutputEndpointLocator().GetInstance(_path);
        }

        public void Connect()
        {
            if (_client != null &&_client.IsConnected)
                _client.Disconnect();
            _client = null;
            isConnected();
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
                _client.Connect(instance.Port, (m) => {
                    var publisherStart = m.IndexOf("|");
                    var publisher = m.Substring(0, publisherStart);
                    var message = m.Substring(publisherStart + 1, m.Length - (publisherStart+1));
                    _handler(publisher, message);
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
    
    class OutputEndpointLocator
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
            var dir = Path.Combine(FS.GetTempPath(), "OpenIDE.Output");
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
