using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Tcp;

namespace CSharp.Responses
{
    public interface IResponseWriter
    {
        void Write(string message);
        void Write(string message, params object[] args);
    }

    public class NullResponseWriter : IResponseWriter
    {
        public void Write(string message) {}
        public void Write(string message, params object[] args) {}
    }

    public class ConsoleResponseWriter : IResponseWriter
    {
        public void Write(string message)
        {
            Console.WriteLine(message);
        }

        public void Write(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }
    }

    public class ServerResponseWriter : IResponseWriter
    {
        private TcpServer _server;
        private Guid _clientID;

        public ServerResponseWriter(TcpServer server, Guid client)
        {
            _server = server;
            _clientID = client;
        }

        public void Write(string message)
        {
            _server.Send(message, _clientID);
        }

        public void Write(string message, params object[] args)
        {
            _server.Send(string.Format(message, args), _clientID);
        }
    }
}
