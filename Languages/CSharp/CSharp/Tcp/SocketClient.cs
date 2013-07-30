using System;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Text;
using System.Threading;
using System.Linq;
namespace CSharp.Tcp
{
	class SocketClient
	{
		private NetworkStream _stream;
        readonly byte[] _buffer = new byte[1000000];
        private int _currentPort;
        private readonly MemoryStream _readBuffer = new MemoryStream();
        private Queue queue = new Queue();
		private bool IsSending = false;
		private Action<string> _onMessage;
		class MessageArgs : EventArgs { public string Message { get; set; } }
		private event EventHandler<MessageArgs> _messageReceived;
		
		public bool IsConnected { get; private set; }
		
		public SocketClient()
		{
			IsConnected = false;
		}

        public void Connect(int port, Action<string> onMessage)
        {
			_onMessage = onMessage;
            Connect(port, 0);
        }

        private void Connect(int port, int retryCount)
        {
            if (retryCount >= 5)
                return;
			try {
	            var client = new TcpClient();
	            client.Connect("127.0.0.1", port);
	            _currentPort = port;
	            _stream = client.GetStream();
	            _stream.BeginRead(_buffer, 0, _buffer.Length, ReadCompleted, _stream);
				IsConnected = true;
			} 
			catch 
			{
                Reconnect(retryCount);
			}
        }

        public void Disconnect()
        {
			try {
				IsConnected = false;
	            _stream.Close();
	            _stream.Dispose();
			}
			catch
			{}
        }

        private void Reconnect(int retryCount)
        {
            retryCount++;
            _readBuffer.SetLength(0);
			Disconnect();
			Connect(_currentPort, retryCount);
		}

        private void ReadCompleted(IAsyncResult result)
        {
            var stream = (NetworkStream)result.AsyncState;
            if (!stream.CanRead)
                return;
            try
            {
                var x = stream.EndRead(result);
                if(x == 0) Reconnect(0);
                for (var i = 0; i < x;i++)
                {
                    if (_buffer[i] == 0)
                    {
                        var data = _readBuffer.ToArray();
                        var actual = Encoding.UTF8.GetString(data, 0, data.Length);
						if (_messageReceived != null)
							_messageReceived(this, new MessageArgs() { Message = actual });
                        else
                            _onMessage(actual);
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
                //WriteError(ex);
                Reconnect(0);
            }
        }


        public void Send(string message)
        {
            if (IsSending)
                throw new Exception("Cannot call send while doing SendAndWait, make up your mind");
            lock (queue)
            {
                queue.Enqueue(message);
                if(!IsSending) {
					SendFromQueue();                      
                }
            }
        }

        public void SendAndWait(string message)
        {
            Send(message);
            IsSending = true;
            var timeout = DateTime.Now;
            while (IsSending && DateTime.Now.Subtract(timeout).TotalMilliseconds < 8000)
                Thread.Sleep(10);
        }

		public string Request(string message)
		{
			string recieved= null;
            var conversationEnded = false;
			var correlationID = "correlationID=" + Guid.NewGuid().ToString() + "|";
			var messageToSend = correlationID + message;
			EventHandler<MessageArgs> msgHandler = (o,a) => {
                    if (a.Message == "EndOfConversation")
                        conversationEnded = true;
                    else
                        _onMessage(a.Message);
                    recieved = a.Message;
				};
			_messageReceived += msgHandler;
			Send(messageToSend);
            while (!conversationEnded)
			{
                Thread.Sleep(10);
			}
			_messageReceived -= msgHandler;
			return recieved;
		}

        private void WriteCompleted(IAsyncResult result)
        {
            var client = (NetworkStream)result.AsyncState;
            try
            {
                client.EndWrite(result);
                lock(queue)
                {
		    		IsSending = false;
                    if (queue.Count > 0)
                        SendFromQueue();
                }
                
            }
            catch (Exception ex)
            {
                WriteError(ex);
				Reconnect(0);
            }
        }

        private void SendFromQueue()
        {
            string message = null;
            lock (queue)
            {
                if (!IsSending && queue.Count > 0)
                    message = queue.Dequeue().ToString();
            }
            if (message != null)
            {
                try
                {
					byte[] toSend = Encoding.UTF8.GetBytes(message).Concat(new byte[] { 0x0 }).ToArray();
                    _stream.BeginWrite(toSend, 0, toSend.Length, WriteCompleted, _stream);
                }
                catch (Exception ex)
                {
                    WriteError(ex);
                }
            }
        }

        private void WriteError(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
	}
}

