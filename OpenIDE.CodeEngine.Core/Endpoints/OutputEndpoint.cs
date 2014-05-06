using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenIDE.CodeEngine.Core.Endpoints.Tcp;
using OpenIDE.Core.FileSystem;

namespace OpenIDE.CodeEngine.Core.Endpoints
{
    public class OutputEndpoint
    {
        private string _token;
        private TcpServer _server;
        private string _instanceFile;

        public int Port { get { return _server.Port; } }

        public OutputEndpoint(string token) {
            _token = token;
            _server = new TcpServer();
            _server.Start();
            writeInstanceInfo();
        }

        public void Send(string publisher, string message) {
            _server.Send(publisher.Trim()+"|"+message);
        }

        public void Stop()
        {
            if (File.Exists(_instanceFile))
                File.Delete(_instanceFile);
        }

        private void writeInstanceInfo()
        {
            var path = Path.Combine(FS.GetTempPath(), "OpenIDE.Output");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            _instanceFile = Path.Combine(path, string.Format("{0}.pid", Process.GetCurrentProcess().Id));
            var sb = new StringBuilder();
            sb.AppendLine(_token);
            sb.AppendLine(_server.Port.ToString());
            File.WriteAllText(_instanceFile, sb.ToString());
        }
    }
}
