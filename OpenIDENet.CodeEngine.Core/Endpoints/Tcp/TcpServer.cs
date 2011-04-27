using System;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
namespace OpenIDENet.CodeEngine.Core.Endpoints.Tcp
{
	public class TcpServer
	{
		private Socket _listener = null;
		private List<NetworkStream> _clients = new List<NetworkStream>();
		private byte[] _buffer = new byte[5000];
		private MemoryStream _readBuffer = new MemoryStream();
		private int _currentPort = 0;
		private string _messageTermination = null;
		
		public event EventHandler ClientConnected;
		public event EventHandler<MessageArgs> IncomingMessage;
		
		public int Port { get { return _currentPort; } }
		
		public TcpServer()
		{
		}
		
		public TcpServer(string messageTermination)
		{
			_messageTermination = messageTermination;
		}
		
		public void Start()
		{
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ipEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            _listener.Bind(ipEndpoint);
            _currentPort = ((IPEndPoint)_listener.LocalEndPoint).Port;
            _listener.Listen(1);
            _listener.BeginAccept(new AsyncCallback(AcceptCallback), _listener);
		}
		
		private void AcceptCallback(IAsyncResult result)
        {
            var listener = (Socket)result.AsyncState;
            try
            {
                var client = listener.EndAccept(result);
                var clientStream = new NetworkStream(client);
                lock (_clients)
                {
                    _clients.Add(clientStream);
                }
                clientStream.BeginRead(_buffer, 0, _buffer.Length, ReadCompleted, clientStream);
                if (ClientConnected != null)
					ClientConnected(this, new EventArgs());
            }
            catch
            {
            }
            finally
            {
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }
        }
		
		private void ReadCompleted(IAsyncResult result)
        {
            var stream = (NetworkStream) result.AsyncState;
            try
            {
                var x = stream.EndRead(result);
                if(x == 0) return;
                for (int i = 0; i < x;i++)
                {
					if (isEndOfMessage(i))
                    {
                        byte[] data = _readBuffer.ToArray();
                        string actual;
						if (_messageTermination == null)
							actual = Encoding.UTF8.GetString(data, 0, data.Length);
						else
						    actual = Encoding.UTF8.GetString(data, 0, data.Length - (_messageTermination.Length - 1));
						if (IncomingMessage != null)
							IncomingMessage(this, new MessageArgs(actual));
                        _readBuffer.SetLength(0);
                    }
                    else
                    {
                        _readBuffer.WriteByte(_buffer[i]);
                    }
                }
                stream.BeginRead(_buffer, 0, _buffer.Length, ReadCompleted, stream);
            }
            catch
            {
                disconnect(stream);
            }
        }
		
		private bool isEndOfMessage(int index)
		{
			if (_messageTermination == null)
				return _buffer[index].Equals(0);
			if (_messageTermination.Length > (index + 1))
				return false;
			for (int i = index; i > (index - _messageTermination.Length); i--)
			{
				if (!Encoding.UTF8.GetString(new byte[] { _buffer[i]}).Equals(_messageTermination.Substring(_messageTermination.Length - (index - i) - 1, 1)))
					return false;
			}
			return true;
		}
		
		private void disconnect(NetworkStream stream)
		{
			lock(_clients)
			{
				_clients.Remove(stream);
			}
		}
		
		public void Send(string message)
        {
            lock (_clients)
			{
				// Add message terminate char
				byte[] data;
				if (_messageTermination == null)
					data = Encoding.UTF8.GetBytes(message).Concat(new byte[] { 0x0 }).ToArray();
				else
					data = Encoding.UTF8.GetBytes(message + _messageTermination).ToArray();
                SendToClients(data);
            }
        }

        private void SendToClients(byte[] data)
        {
			var failingClients = new List<NetworkStream>();
            foreach (var client in _clients)
            {
                try
                {
                    var stream = client;
                    client.BeginWrite(data, 0, data.Length, WriteCompleted, stream);
                }
                catch
                {
                    failingClients.Add(client);
                }
            }
			failingClients.ForEach(client => disconnect(client));
        }
		
		private void WriteCompleted(IAsyncResult result)
        {
            var client = (NetworkStream) result.AsyncState;
            try
            {
                client.EndWrite(result);
            }
            catch
            {
                disconnect(client);
            }
        }
	}
}